using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using MorkoBotRavenEdition.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;

namespace MorkoBotRavenEdition.Services {
    internal class WikiService
    {
        private readonly DiscordSocketClient _socketClient;
        private readonly UserService _userService;
        private readonly ILogger _logger;
        private readonly string _wikiUrl;

        private static readonly Random Random = new Random();

        public WikiService(DiscordSocketClient socketClient, UserService userService, ILogger logger)
        {
            _socketClient = socketClient;
            _userService = userService;
            _logger = logger;
            _wikiUrl = ConfigurationManager.AppSettings.Get("WikiUrl");
        }

        public void Start(IReceiverClient queueClient)
        {
            queueClient.RegisterMessageHandler(async (message, token) => { await HandleMessage(message); }, (exception) => { _logger.LogError(exception.Exception, @"Caught an exception from the message queue."); return Task.CompletedTask; });
            Console.WriteLine(@"[MODULE] Wiki message bus handler registered.");
        }

        /// <summary>
        /// Handles a message recieved from the wiki service bus.
        /// </summary>
        private async Task HandleMessage(Message message)
        {
            var guild = _socketClient.GetGuild(MorkoBot.DefaultGuild);
            if (guild == null)
                return;

            var user = new List<SocketGuildUser>(guild.Users)
                .Find(c => c.Username == (string)message.UserProperties["username"]);

            if (user == null) return;
            if (!message.UserProperties.ContainsKey("isMinor"))
            {
                var profile = await _userService.GetProfile(user.Id, guild.Id);
                var xpEarned = Random.Next(25, 50);
                var ocEarned = Random.Next(1, 5);

                await _userService.AddExperience(profile, xpEarned);

                profile.OpenSewerTokens += ocEarned;
                await _userService.SaveProfile(profile);

                var builder = new EmbedBuilder();
                builder.WithTitle(@"Wiki Contribution");
                builder.WithDescription($@"{user.Username} has made a contribution to the Stalburg Wiki, and earned {xpEarned} <:olut:329889326051753986> XP and {ocEarned} <:sewercoin:354606163112755230> OC!");
                if (message.UserProperties.ContainsKey("summary") && !string.IsNullOrWhiteSpace((string)message.UserProperties["summary"]))
                    builder.AddField(@"Summary", message.UserProperties["summary"]);

                //builder2.AddField("Characters added", "+" + size.ToString());
                builder.WithColor(Color.Green);
                builder.WithUrl((string)message.UserProperties["url"]);
                builder.WithAuthor(user);

                var channel = (SocketTextChannel) guild.GetChannel(Convert.ToUInt64(ConfigurationManager.AppSettings.Get("WikiChannelId")));
                await channel.SendMessageAsync("", false, builder.Build());
            }
        }

        /// <summary>
        /// Returns a map information object from the specified map name.
        /// Returns null if the page is not found.
        /// </summary>
        public async Task<InfraMap> GetMapInformationAsync(string mapName)
        {
            var pageNode = GetPageNode(await GetPage(mapName));
            if (pageNode == null) throw new Exception("The page was not found, or MorkoBot could not connect to the wiki server.");

            var intNode = GetIntegrationNode(pageNode);
            if (intNode == null) throw new Exception("The wiki page is not integrated with MorkoBot. Please see the wiki documentation.");

            var infobox = GetInfobox(pageNode);
            if (infobox == null) throw new Exception("The map infobox was not found. An infobox must exist for statistics to be downloaded.");

            //var name = GetInfoboxValue(infobox, "Map Name");

            var map = new InfraMap(mapName)
            {
                WikiUrl = _wikiUrl + "/" + mapName,
                ThumbUrl = GetMapThumbAddress(pageNode),
                BspName = GetInfoboxValue(infobox, @"BSP Name").Replace("\n", string.Empty),
                PhotoSpots = Convert.ToInt32(GetInfoboxValue(infobox, @"Photo Spots")),
                CorruptionSpots = Convert.ToInt32(GetInfoboxValue(infobox, @"Corruption Spots")),
                RepairSpots = Convert.ToInt32(GetInfoboxValue(infobox, @"Repair Spots")),
                MistakeSpots = Convert.ToInt32(GetInfoboxValue(infobox, @"Mistake Spots")),
                Geocaches = Convert.ToInt32(GetInfoboxValue(infobox, @"Geocaches")),
                FlowMeters = Convert.ToInt32(GetInfoboxValue(infobox, @"Water Flow Meters"))
            };

            return map;
        }

        /// <summary>
        /// Returns the main page node from the wiki page.
        /// </summary>
        private static HtmlNode GetPageNode(HtmlDocument html) {
            return html?.DocumentNode?.SelectSingleNode("//html//body//div[contains(@id, 'content')]//div[contains(@id, 'bodyContent')]//div[contains(@id, 'mw-content-text')]");
        }

        /// <summary>
        /// Finds the bot integration node, if present, from the wiki page.
        /// </summary>
        private static HtmlNode GetIntegrationNode(HtmlNode pageNode) { return pageNode.SelectSingleNode(@"//div[contains(@id, 'bot-integration')]"); }
        private static HtmlNode GetInfobox(HtmlNode pageNode) { return pageNode.SelectSingleNode(@"//table[contains(@class, 'infobox')]"); }

        /// <summary>
        /// Returns the value of a key in a wiki infobox element.
        /// </summary>
        /// <param name="infobox">Node containing the infobox table.</param>
        /// <param name="key">Key (plain text name) of the infobox field.</param>
        private static string GetInfoboxValue(HtmlNode infobox, string key) {
            var node = infobox.SelectSingleNode(@"//tr[./th/text() = '" + key + "']//td");

            return node?.InnerText.Replace(@"\n", string.Empty);

            // Strip newlines
        }
        
        /// <summary>
        /// Returns the thumbnail URL of a map page.
        /// </summary>
        private string GetMapThumbAddress(HtmlNode infoboxNode)
        {
            var imageNode = infoboxNode.SelectSingleNode(@"//tr//td//a//img[@alt[starts-with(., 'mapimage:') and string-length() > 9]]");
            if (imageNode == null) return null;
            if (imageNode.Attributes.Contains("src"))
                return _wikiUrl + imageNode.Attributes["src"].Value;

            return null;

        }

        /// <summary>
        /// Retrieves a wiki page from the specified page name.
        /// </summary>
        /// <param name="page">Name of the page.</param>
        /// <returns>Raw HtmlDocument containing the page.</returns>
        private async Task<HtmlDocument> GetPage(string page)
        {
            try
            {
                var url = _wikiUrl + "/" + page;
                var req = WebRequest.Create(url);
                var response = await req.GetResponseAsync();
                var rstream = response.GetResponseStream();

                string content;
                using (var str = new StreamReader(rstream ?? throw new InvalidOperationException()))
                {
                    content = await str.ReadToEndAsync();
                }

                var html = new HtmlDocument();

                html.LoadHtml(content);

                return html;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
