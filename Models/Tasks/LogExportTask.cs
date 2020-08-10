using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using MorkoBotRavenEdition.Services;
using MorkoBotRavenEdition.Models.Guild;
using Discord;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

namespace MorkoBotRavenEdition.Models.Tasks
{
    internal class LogExportTask {
        private readonly IDiscordClient _client;
        private readonly AmazonS3Client _s3Client;
        private readonly BotDbContext _context;
        private long _guild;
        private long? _channel;
        private DateTime? _start;
        private DateTime? _end;

        private static readonly HttpClient _httpClient = new HttpClient();

        private const string BULK_EXPORT_S3_BUCKET = "morkobot-exports";
        private const string BULK_EXPORT_S3_REGION = "eu-west-2";

        private readonly IDictionary<long, string> _userCache 
            = new Dictionary<long, string>();
        private readonly IDictionary<long, string> _channelCache
            = new Dictionary<long, string>();

        public LogExportTask(IDiscordClient client, AmazonS3Client s3Client, BotDbContext context) {
            _client = client;
            _s3Client = s3Client;
            _context = context;
        }

        internal async Task<string> RunAsync(long guild, long? channel, DateTime? start, DateTime? end) {
            _guild = guild;
            _channel = channel;
            _start = start;
            _end = end;

            var channels = (await _context.LoggedMessages
                .Where(m => 
                    m.Guild == _guild && 
                    (!channel.HasValue || m.Channel == channel) && 
                    (!start.HasValue || (m.TimeStamp.Date >= start.Value.Date)) && 
                    (!end.HasValue || (m.TimeStamp.Date <= end.Value.Date)) && 
                    m.Author != (long)_client.CurrentUser.Id)
                .OrderBy(m => m.TimeStamp).ToListAsync())
                .GroupBy(m => m.Channel)
                .Select(m => new { Channel = m.Key, Messages = m.ToList() });

            if (!channels.Any()) return null;

            var logs = new List<(long, string)>();
            foreach (var c in channels)  {
                logs.Add((c.Channel, await BuildChannelLogAsync(c.Messages)));
            }

            // use hastebin if only one, and small enough
            if (logs.Count() == 1 && logs.First().Item2.Length < 200) {
                var log = logs.First();
                return await UploadLogToHastebinAsync(log.Item1, log.Item2);
            } else {
                var urls = new List<(long, string)>();
                
                foreach (var l in logs) {
                    urls.Add((l.Item1, await UploadLogToS3Async(l.Item1, l.Item2)));
                }

                var index = await BuildS3IndexFileAsync(urls);

                // upload index file
                try {
                    var key = $"exports/{_guild}/index.html";
                    var request = new PutObjectRequest() {
                        BucketName = BULK_EXPORT_S3_BUCKET,
                        Key = key,
                        CannedACL = S3CannedACL.NoACL,
                        ContentBody = index
                    };
                    var response = await _s3Client.PutObjectAsync(request);
                    var url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest() {
                        BucketName = BULK_EXPORT_S3_BUCKET, 
                        Key = key,
                        Expires = DateTime.Now.AddDays(2)
                    });

                    return url;
                } catch (AmazonS3Exception e) {
                    // TODO: log
                    throw;
                }
            }
        }

        private async Task<string> BuildChannelLogAsync(IEnumerable<LoggedMessage> messages) {
            var builder = new StringBuilder();

            builder.AppendLine("=== MSIC EXPORT BEGIN ===");
            builder.AppendLine();
            builder.AppendLine($"Guild {_guild}");
            builder.AppendLine($"{messages.Count()} messages");
            builder.AppendLine();

            foreach (var message in messages) {
                var user = await ResolveUserAsync(message.Author);

                // Fallback to the ID if we can't resolve the author's username.
                var authorName =
                    user != null 
                    ? user
                    : message.Author.ToString();
                var editInfo = "";
                if (message.OriginalId != 0) {
                    editInfo += $" (edit of {message.OriginalId})";
                }
                if (message.Deleted) {
                    editInfo += " (deleted)";
                }

                var content = await ResolveDiscordReferencesAsync(message.Content);
                builder.AppendLine($"{message.TimeStamp} {authorName}{editInfo}: {content}");
            }

            return builder.ToString();
        }

        private async Task<string> ResolveUserAsync(long id) {
            if (!_userCache.ContainsKey(id)) {
                var user = await _client.GetUserAsync((ulong)id);
                if (user == null) {
                    _userCache[id] = null;
                } else {
                    _userCache[id] = $"{user.Username}#{user.Discriminator}";
                }
            }

            return _userCache[id];
        }

        private async Task<string> ResolveDiscordReferencesAsync(string text) {
            var mentions = Regex.Matches(text, "(?<=<@)(.*)(?=>)");
            foreach (var mention in mentions) {
                if (!long.TryParse(mention.ToString(), out var userId)) continue;
                var user = await ResolveUserAsync(userId);

                if (string.IsNullOrWhiteSpace(user)) continue;
                text = text.Replace($"<@{mention}>", $"@{user}");
            }

            var channels = Regex.Matches(text, "(?<=<#)(.*)(?=>)");
            foreach (var channel in channels) {
                if (!long.TryParse(channel.ToString(), out var channelId)) continue;
                var channelName = await ResolveChannelNameAsync(channelId);

                if (string.IsNullOrWhiteSpace(channelName)) continue;
                text = text.Replace($"<#{channel}>", $"#{channelName}");
            }

            return text;
        }

        private async Task<string> BuildS3IndexFileAsync(IList<(long, string)> channels) {
            var builder = new StringBuilder();
            builder.AppendLine("<html>\n<table>");
            builder.AppendLine("<tr>\n<th>Channel</th>\n<th>Download</th>\n</tr>");
            foreach (var channel in channels) {
                var channelName = await ResolveChannelNameAsync(channel.Item1);
                if (channelName == null) channelName = "[unresolved]";
                builder.AppendLine($"<tr>\n<td>#{channelName}</td>\n<td><a href=\"{channel.Item2}\">{channel.Item1}.txt.gz</a></td>");
            }
            
            builder.AppendLine("</table>\n</html>");
            return builder.ToString();
        }

        private async Task<string> ResolveChannelNameAsync(long id) {
            // get the channel name
            if (!_channelCache.ContainsKey(id)) {
                var channel = await _client.GetChannelAsync((ulong)id);
                if (channel == null) {
                    _channelCache[id] = null;
                } else {
                    _channelCache[id] = channel.Name;
                }
            }

            return _channelCache[id];
        }

        private async Task<string> UploadLogToHastebinAsync(long channel, string log) {
            var content = new StringContent(log, Encoding.UTF8, "text/plain");
            var response = await _httpClient.PostAsync("https://hastebin.com/documents", content);

            if (!response.IsSuccessStatusCode) return null;
            var resp = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(resp)) return null;

            var key = (string)JsonConvert.DeserializeObject<dynamic>(resp).key;
            return $"https://hastebin.com/{key}";
        }

        private async Task<string> UploadLogToS3Async(long channel, string log) {
            var key = $"exports/{_guild}/{channel}.txt.gz";

            var request = new PutObjectRequest() {
                BucketName = BULK_EXPORT_S3_BUCKET,
                Key = key,
                CannedACL = S3CannedACL.NoACL
            };

            using (var reader = new MemoryStream())
            using (var writer = new MemoryStream()) {
                using (var sw = new StreamWriter(reader, Encoding.UTF8, 1024, true))
                    await sw.WriteAsync(log);
                reader.Seek(0, SeekOrigin.Begin);
                
                using (var gzip = new GZipStream(writer, CompressionLevel.Fastest, true)) {
                    await reader.CopyToAsync(gzip);
                }

                writer.Seek(0, SeekOrigin.Begin);
                request.InputStream = writer;

                try {
                    var response = await _s3Client.PutObjectAsync(request);
                    var url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest() {
                        BucketName = BULK_EXPORT_S3_BUCKET, 
                        Key = key,
                        Expires = DateTime.Now.AddDays(2)
                    });

                    return url;
                } catch (AmazonS3Exception e) {
                    // log some error
                    //_logger.LogError("Log export failed");
                    //_logger.LogError(e.ToString());
                    throw;
                }
            }
        }
    }
}