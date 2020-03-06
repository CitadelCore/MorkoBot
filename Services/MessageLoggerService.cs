using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models.Guild;
using Newtonsoft.Json;

namespace MorkoBotRavenEdition.Services
{
    internal class MessageLoggerService
    {
        private static BotDbContext _context;
        private static readonly HttpClient _httpClient = new HttpClient();

        public MessageLoggerService(BotDbContext context)
        {
            _context = context;
        }

        public async Task LogSend(SocketMessage message)
        {
            var logged = LoggedMessage.FromSocketMessage(message);

            _context.LoggedMessages.Add(logged);
            await _context.SaveChangesAsync();
        }

        public async Task LogUpdate(SocketMessage message, ulong originalId)
        {
            var logged = LoggedMessage.FromSocketMessage(message);
            logged.OriginalId = (long) originalId;

            _context.LoggedMessages.Add(logged);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Exports a log to Hastebin and returns the path as a URL
        /// </summary>
        /// <param name="date">The date to filter by</param>
        /// <returns>Link to the paste, otherwise null if failure</returns>
        public async Task<string> LogExportAsync(IDiscordClient client, long guild, long channel, DateTime start, DateTime end) 
        {
            var builder = new StringBuilder();
            builder.AppendLine($"=== MSIC LOG BEGIN {start} END {end}  ===");

            var messages = _context.LoggedMessages
                .Where(m => 
                    m.Guild == guild && 
                    m.Channel == channel && 
                    m.TimeStamp.Date >= start.Date && 
                    m.TimeStamp.Date <= end.Date && 
                    m.Author != (long)client.CurrentUser.Id)
                .OrderBy(m => m.TimeStamp);
            builder.AppendLine($"Guild {guild}");
            builder.AppendLine($"Channel {channel}");
            builder.AppendLine($"{messages.Count()} messages");
            builder.AppendLine();

            var authorCache = new Dictionary<long, string>();
            foreach (var message in messages) {
                if (!authorCache.ContainsKey(message.Author)) {
                    var author = await client.GetUserAsync((ulong)message.Author);
                    authorCache[message.Author] = $"{author.Username}#{author.Discriminator}";
                }

                var authorName = authorCache[message.Author];
                var editInfo = "";
                if (message.OriginalId != 0) {
                    editInfo = $" (editof {message.OriginalId})";
                }

                builder.AppendLine($"{message.TimeStamp} {authorName}{editInfo}: {message.Content}");
            }

            var content = new StringContent(builder.ToString(), Encoding.UTF8, "text/plain");
            var response = await _httpClient.PostAsync("https://hastebin.com/documents", content);

            if (!response.IsSuccessStatusCode) return null;
            var resp = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(resp)) return null;

            var key = (string)JsonConvert.DeserializeObject<dynamic>(resp).key;
            return $"https://hastebin.com/{key}";
        }
    }
}
