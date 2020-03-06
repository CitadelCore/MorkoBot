using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Services;
using MorkoBotRavenEdition.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MorkoBotRavenEdition.Modules
{
    [Summary("Game Module")]
    [Description("Fun commands to relieve the tedium!")]
    [Group("game")]
    internal class GameModule : ModuleBase
    {
        private static readonly Random Random = new Random();
        private readonly UserService _userService;
        private readonly GuildInfoService _infoService;

        // Increment specific vars
        private static IUserMessage _incrementCache;
        private const int INCREMENT_XP = 25;

        public GameModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userService = serviceProvider.GetService<UserService>();
            _infoService = serviceProvider.GetService<GuildInfoService>();
        }

        [Command("increment"), Summary(@"Increments your increment counter.")]
        public async Task IncrementAsync()
        {
            var profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);

            if (!(DateTime.Now >= profile.LastIncremented + TimeSpan.FromHours(1)))
            {
                var minutesLeft = (profile.LastIncremented + TimeSpan.FromHours(1) - DateTime.Now).Minutes;

                await Context.Channel.SendMessageAsync(string.Empty, false, 
                    GetResponseEmbed($"You cannot increment again yet because you have already incremented in the last hour. {minutesLeft} minutes left until you can increment again.", Color.Red).Build());

                return;
            }

            await _userService.AddExperience(profile, INCREMENT_XP);
            await UpdateGuildIncrement();

            profile.LastIncremented = DateTime.Now;
            profile.IncrementCount += 1;
            await _userService.SaveProfile(profile);
        }

        [Command("increment info"), Summary(@"Retrieves information about the current increment counter.")]
        public async Task IncrementInfoAsync()
        {
            var guildInfo = await _infoService.GetGuildInfo(Context.Guild.Id);

            var builder = new EmbedBuilder();
            builder.WithTitle(@"Game Module");
            builder.WithDescription(@"Showing current increment statistics.");
            builder.WithColor(Color.Green);
            builder.AddField(@"Current Count", guildInfo.IncrementCount, true);
            builder.AddField(@"Target Count", guildInfo.IncrementTarget, true);
            builder.AddField(@"Increments Left", guildInfo.IncrementTarget - guildInfo.IncrementCount, true);

            await Context.Channel.SendMessageAsync(string.Empty, false, builder.Build());
        }

        [Command("increment reset"), Summary(@"Can be used by moderators to reset a user's increment cooldown.")]
        [PermitRoles("Discord Moderator")]
        public async Task IncrementResetAsync([Summary(@"The user to reset.")] IUser user)
        {
            var profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);
            
            profile.LastIncremented = DateTime.Now - TimeSpan.FromHours(1);
            await _userService.SaveProfile(profile);

            await Context.Channel.SendMessageAsync(string.Empty, false, GetResponseEmbed(@"Successfully reset the user's increment cooldown.", Color.Green).Build());
        }

        /// <summary>
        /// Updates the guild's increment count,
        /// and triggers events if it's time.
        /// </summary>
        private async Task UpdateGuildIncrement()
        {
            var guildInfo = await _infoService.GetGuildInfo(Context.Guild.Id);
            guildInfo.IncrementCount += 1;

            // Counter has reached its maximum.
            if (guildInfo.IncrementCount > guildInfo.IncrementTarget)
            {
                guildInfo.IncrementCount = 0;
                guildInfo.IncrementTarget *= 2;
                await _infoService.SaveGuildInfo(guildInfo);

                var targetBuilder = new EmbedBuilder();
                targetBuilder.WithTitle(@"Game Module");
                targetBuilder.WithDescription(@"The increment target has been reached. The increment counter has been reset and the target count has been doubled.");
                targetBuilder.WithColor(Color.Green);
                targetBuilder.AddField(@"New Target", guildInfo.IncrementTarget, true);

                await Context.Channel.SendMessageAsync(string.Empty, false, targetBuilder.Build());
                return;
            }

            await _infoService.SaveGuildInfo(guildInfo);

            if (_incrementCache != null)
                await _incrementCache.DeleteAsync();

            var builder = new EmbedBuilder();
            builder.WithTitle(@"Game Module");
            builder.WithDescription(@"The server-wide increment count has been updated.");
            builder.WithColor(Color.Green);
            builder.AddField(@"Previous Count", guildInfo.IncrementCount - 1, true);
            builder.AddField(@"New Count", guildInfo.IncrementCount, true);
            builder.AddField(@"Target Count", guildInfo.IncrementTarget, true);

            _incrementCache = await Context.Channel.SendMessageAsync(string.Empty, false, builder.Build());
        }
    }
}
