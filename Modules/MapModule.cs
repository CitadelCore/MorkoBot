using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Attributes;

namespace MorkoBotRavenEdition.Modules
{
    [InfraExclusive]
    [Summary("INFRA Map Module")]
    [Description("Get information about INFRA maps!")]
    [Group("map")]
    internal class MapModule : ModuleBase
    {
        private readonly WikiService _wikiService;
        public MapModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            // All maps are now retrieved from the wiki
            _wikiService = serviceProvider.GetService<WikiService>();
        }

        [Command("info")]
        [Summary(@"Returns information from an INFRA map.")]
        public async Task InfoAsync([Remainder] [Summary(@"The map to get information about.")] string mapName)
        {
            // Get basic map information
            var map = await _wikiService.GetMapInformationAsync(mapName.ToLower());

            if (map == null)
                throw new Exception(@"The map was null or not found.");

            // Add general fields
            var builder = new EmbedBuilder();
            builder.WithTitle($@"Showing information for map {map.Name}.");
            builder.WithDescription(@"Shows map information for a specific INFRA map. Map statistics are fetched from the INFRA wiki.");
            builder.WithColor(Color.Green);
            builder.WithUrl(map.WikiUrl);
            builder.WithThumbnailUrl(map.ThumbUrl);
            builder.WithAuthor(Context.User);

            // Add statistic fields
            builder.AddField(@"BSP Name", map.BspName, true);
            builder.AddField(@"Photo Spots", map.PhotoSpots, true);
            builder.AddField(@"Corruption Spots", map.CorruptionSpots, true);
            builder.AddField(@"Repair Spots", map.RepairSpots, true);
            builder.AddField(@"Mistake Spots", map.MistakeSpots, true);
            builder.AddField(@"Geocaches", map.Geocaches, true);
            builder.AddField(@"Flow Meters", map.FlowMeters, true);

            builder.WithFooter(@"Information provided by the Stalburg Wiki.");

            await ReplyAsync(string.Empty, false, builder.Build());
        }
    }
}
