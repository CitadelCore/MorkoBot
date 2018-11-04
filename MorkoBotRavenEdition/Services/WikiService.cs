using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HtmlAgilityPack;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Modules;
using MorkoBotRavenEdition.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Services {
    class WikiService
    {
        private DiscordSocketClient _socketClient;
        private UserService _userService;
        private ILogger _logger;
        private string _wikiUrl;

        private static Random random = new Random();

        public WikiService(DiscordSocketClient socketClient, QueueClient queueClient, UserService userService, ILogger logger)
        {
            _socketClient = socketClient;
            _userService = userService;
            _logger = logger;
            _wikiUrl = ConfigurationManager.AppSettings.Get("WikiUrl");

            queueClient.RegisterMessageHandler(async (message, token) => { await HandleMessage(message); }, (exception) => { logger.LogError(exception.Exception, "Caught an exception from the message queue."); return Task.CompletedTask; });
            Console.WriteLine("[MODULE] Wiki message bus handler registered.");
        }

        /// <summary>
        /// Handles a message recieved from the wiki service bus.
        /// </summary>
        private async Task HandleMessage(Message message)
        {
            SocketGuild guild = _socketClient.GetGuild(MorkoBot.DefaultGuild);

            SocketGuildUser user = new List<SocketGuildUser>(guild.Users)
                .Find(c => c.Username == ((string)message.UserProperties["username"]));

            if (user == null) return;
            if (!message.UserProperties.ContainsKey("isMinor"))
            {
                UserProfile profile = await _userService.GetProfile(user.Id, guild.Id);
                int xpEarned = random.Next(25, 50);
                int ocEarned = random.Next(1, 5);

                await _userService.AddExperience(profile, xpEarned);

                profile.OpenSewerTokens += ocEarned;
                await _userService.SaveProfile(profile);

                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle("Wiki Contribution");
                builder.WithDescription(user.Username + String.Format(" has made a contribution to the Stalburg Wiki, and earned {0} <:olut:329889326051753986> XP and {1} <:sewercoin:354606163112755230> OC!", xpEarned, ocEarned));
                if (message.UserProperties.ContainsKey("summary") && !String.IsNullOrEmpty((string)message.UserProperties["summary"]))
                    builder.AddField("Summary", message.UserProperties["summary"]);

                //builder2.AddField("Characters added", "+" + size.ToString());
                builder.WithColor(Color.Green);
                builder.WithUrl((string)message.UserProperties["url"]);
                builder.WithAuthor(user);

                SocketTextChannel channel = (SocketTextChannel) guild.GetChannel(Convert.ToUInt64(ConfigurationManager.AppSettings.Get("WikiChannelId")));
                await channel.SendMessageAsync("", false, builder.Build());
            }
        }

        /// <summary>
        /// Returns a map information object from the specified map name.
        /// Returns null if the page is not found.
        /// </summary>
        public async Task<InfraMap> GetMapInformationAsync(string mapName)
        {
            HtmlNode pageNode = GetPageNode(await GetPage(mapName));
            if (pageNode == null) throw new Exception("The page was not found, or MorkoBot could not connect to the wiki server.");

            HtmlNode intNode = GetIntegrationNode(pageNode);
            if (intNode == null) throw new Exception("The wiki page is not integrated with MorkoBot. Please see the wiki documentation.");

            HtmlNode infobox = GetInfobox(pageNode);
            if (infobox == null) throw new Exception("The map infobox was not found. An infobox must exist for statistics to be downloaded.");

            string _name = GetInfoboxValue(infobox, "Map Name");

            InfraMap map = new InfraMap(mapName)
            {
                WikiUrl = _wikiUrl + "/" + mapName,
                ThumbUrl = GetMapThumbAddress(pageNode),
                BspName = GetInfoboxValue(infobox, "BSP Name").Replace("\n", String.Empty),
                PhotoSpots = Convert.ToInt32(GetInfoboxValue(infobox, "Photo Spots")),
                CorruptionSpots = Convert.ToInt32(GetInfoboxValue(infobox, "Corruption Spots")),
                RepairSpots = Convert.ToInt32(GetInfoboxValue(infobox, "Repair Spots")),
                MistakeSpots = Convert.ToInt32(GetInfoboxValue(infobox, "Mistake Spots")),
                Geocaches = Convert.ToInt32(GetInfoboxValue(infobox, "Geocaches")),
                FlowMeters = Convert.ToInt32(GetInfoboxValue(infobox, "Water Flow Meters"))
            };

            return map;
        }

        /// <summary>
        /// Returns the main page node from the wiki page.
        /// </summary>
        public static HtmlNode GetPageNode(HtmlDocument html) {
            if (html == null || html.DocumentNode == null) return null;
            return html.DocumentNode.SelectSingleNode("//html//body//div[contains(@id, 'content')]//div[contains(@id, 'bodyContent')]//div[contains(@id, 'mw-content-text')]");
        }

        /// <summary>
        /// Finds the bot integration node, if present, from the wiki page.
        /// </summary>
        public static HtmlNode GetIntegrationNode(HtmlNode pageNode) { return pageNode.SelectSingleNode(@"//div[contains(@id, 'bot-integration')]"); }
        public static HtmlNode GetInfobox(HtmlNode pageNode) { return pageNode.SelectSingleNode(@"//table[contains(@class, 'infobox')]"); }

        /// <summary>
        /// Returns the value of a key in a wiki infobox element.
        /// </summary>
        /// <param name="infobox">Node containing the infobox table.</param>
        /// <param name="key">Key (plain text name) of the infobox field.</param>
        public static string GetInfoboxValue(HtmlNode infobox, string key) {
            HtmlNode node = infobox.SelectSingleNode(@"//tr[./th/text() = '" + key + "']//td");

            if (node == null) return null;

            // Strip newlines
            return node.InnerText.Replace(@"\n", String.Empty);
        }

        /// <summary>
        /// Returns the thumbnail URL of a map page.
        /// </summary>
        public string GetMapThumbAddress(HtmlNode infoboxNode)
        {
            HtmlNode imageNode = infoboxNode.SelectSingleNode(@"//tr//td//a//img[@alt[starts-with(., 'mapimage:') and string-length() > 9]]");
            if (imageNode != null)
            {
                if (imageNode.Attributes.Contains("src"))
                    return _wikiUrl + imageNode.Attributes["src"].Value;

                return null;
            }
            else return null;
        }

        /// <summary>
        /// Retrieves a wiki page from the specified page name.
        /// </summary>
        /// <param name="page">Name of the page.</param>
        /// <returns>Raw HtmlDocument containing the page.</returns>
        public async Task<HtmlDocument> GetPage(string page)
        {
            try
            {
                string URL = _wikiUrl + "/" + page;

                WebRequest req = WebRequest.Create(URL);
                WebResponse response = await req.GetResponseAsync();
                Stream rstream = response.GetResponseStream();

                string content = null;

                using (StreamReader str = new StreamReader(rstream))
                {
                    content = await str.ReadToEndAsync();
                }

                HtmlDocument html = new HtmlDocument();

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
