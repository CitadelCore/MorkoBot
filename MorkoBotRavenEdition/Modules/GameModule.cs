using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Services;
using MorkoBotRavenEdition.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Modules
{
    [Summary("Game Module")]
    class GameModule : MorkoModuleBase
    {
        private static Random random = new Random();
        private readonly UserService _userService;
        private readonly GuildInfoService _infoService;

        // Increment specific vars
        private static IUserMessage incrementCache;
        private static readonly int incrementXP = 25;
        
        // Other specific vars
        private static IList<string> VideoUris = new List<string>()
        {
            "https://youtu.be/330YX8TWNWA",
            "https://youtu.be/e_sk8t4XNV0",
            "https://youtu.be/ZZ5BTvfmQu4",
            "https://youtu.be/yIDKH8AuK90",
            "https://www.youtube.com/watch?v=UjgLRrx2pOo",
            "https://www.youtube.com/watch?v=EbYa7TQpzzI",
            "https://www.youtube.com/watch?v=lQ224V6KurY",
            "https://www.youtube.com/watch?v=_EYoLV-PQk8",
            "https://www.youtube.com/watch?v=z8bIXeM0RJ8",
            "https://www.youtube.com/watch?v=Tv8BP8hHEdw",
            "https://www.youtube.com/watch?v=1yZ_i1u7r8Y",
            "https://www.youtube.com/watch?v=Eu26QvQzoOs",
            "https://www.youtube.com/watch?v=TwiZ36EMgTc",
            "https://www.youtube.com/watch?v=JqgyXqROa20",
            "https://www.youtube.com/watch?v=3EoAkQgLtGw",
            "https://www.youtube.com/watch?v=u-qy15sc9Qw",
            "https://www.youtube.com/watch?v=7kAa4IeNGxU",
            "https://www.youtube.com/watch?v=_s4Byjvc-hs"
        };

        public GameModule(UserService userService, GuildInfoService infoService)
        {
            _userService = userService;
            _infoService = infoService;
        }

        [Command("nullifact"), Summary("Nullifacts a user. Use with caution.")]
        public async Task NullifactAsync([Summary("The user to nullifact.")] IUser user)
        {
            await Context.Channel.SendMessageAsync(String.Format("{0}, prepare for nullifaction. <:5pm:423182998343516170>\nVerdict has been authenticated and will be executed now.", user.Mention));
            await Task.Delay(2000);

            // Send the video payload
            string video = VideoUris[random.Next(0, VideoUris.Count)];
            await Context.Channel.SendMessageAsync(video);
        }

        [Command("increment"), Summary("Increments your increment counter.")]
        public async Task IncrementAsync()
        {
            UserProfile profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);

            if (profile.LastIncremented != null && !(DateTime.Now >= (profile.LastIncremented + TimeSpan.FromHours(1))))
            {
                await MessageUtilities.SendPMSafely(Context.User, Context.Channel, String.Empty, false, 
                    GetResponseEmbed(String.Format("You cannot increment again yet because you have already incremented in the last hour. {0} minutes left until you can increment again.",
                    (DateTime.Now - (profile.LastIncremented + TimeSpan.FromHours(1))).Minutes), Color.Red).Build());
                return;
            }

            await _userService.AddExperience(profile, incrementXP);
            await UpdateGuildIncrement();

            profile.LastIncremented = DateTime.Now;
            profile.IncrementCount += 1;
            await _userService.SaveProfile(profile);
        }

        [Command("increment info"), Summary("Retrieves information about the current increment counter.")]
        public async Task IncrementInfoAsync()
        {
            ExtendedGuildInfo guildInfo = await _infoService.GetGuildInfo(Context.Guild.Id);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Game Module");
            builder.WithDescription("Showing current increment statistics.");
            builder.WithColor(Color.Green);
            builder.AddField("Current Count", guildInfo.IncrementCount, true);
            builder.AddField("Target Count", guildInfo.IncrementTarget, true);
            builder.AddField("Increments Left", guildInfo.IncrementTarget - guildInfo.IncrementCount, true);

            await Context.Channel.SendMessageAsync(String.Empty, false, builder.Build());
        }

        [Command("increment reset"), Summary("Can be used by moderators to reset a user's increment cooldown.")]
        [PermitRoles("Discord Moderator")]
        public async Task IncrementResetAsync([Summary("The user to reset.")] IUser user)
        {
            UserProfile profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);

            profile.LastIncremented = DateTime.Now - TimeSpan.FromHours(1);
            await _userService.SaveProfile(profile);

            await MessageUtilities.SendPMSafely(Context.User, Context.Channel, String.Empty, false, GetResponseEmbed("Successfully reset the user's increment cooldown.", Color.Green).Build());
        }

        /// <summary>
        /// Updates the guild's increment count,
        /// and triggers events if it's time.
        /// </summary>
        private async Task UpdateGuildIncrement()
        {
            ExtendedGuildInfo guildInfo = await _infoService.GetGuildInfo(Context.Guild.Id);
            guildInfo.IncrementCount += 1;

            // Counter has reached its maximum.
            if (guildInfo.IncrementCount > guildInfo.IncrementTarget)
            {
                guildInfo.IncrementCount = 0;
                guildInfo.IncrementTarget = guildInfo.IncrementTarget * 2;
                await _infoService.SaveGuildInfo(guildInfo);

                EmbedBuilder targetBuilder = new EmbedBuilder();
                targetBuilder.WithTitle("Game Module");
                targetBuilder.WithDescription("The increment target has been reached. The increment counter has been reset and the target count has been doubled.");
                targetBuilder.WithColor(Color.Green);
                targetBuilder.AddField("New Target", guildInfo.IncrementTarget, true);

                await Context.Channel.SendMessageAsync(String.Empty, false, targetBuilder.Build());
                return;
            }

            await _infoService.SaveGuildInfo(guildInfo);

            if (incrementCache != null)
                await incrementCache.DeleteAsync();

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Game Module");
            builder.WithDescription("The server-wide increment count has been updated.");
            builder.WithColor(Color.Green);
            builder.AddField("Previous Count", guildInfo.IncrementCount - 1, true);
            builder.AddField("New Count", guildInfo.IncrementCount, true);
            builder.AddField("Target Count", guildInfo.IncrementTarget, true);

            incrementCache = await Context.Channel.SendMessageAsync(String.Empty, false, builder.Build());
        }
    }
}
