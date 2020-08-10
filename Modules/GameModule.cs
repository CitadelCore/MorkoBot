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
    internal class GameModule : ModuleBase
    {
        private static readonly Random Random = new Random();
        private readonly UserService _userService;
        private readonly GuildInfoService _infoService;
        private readonly GPT2Service _gpt2Service;

        // Increment specific vars
        private static IUserMessage _incrementCache;
        private const int INCREMENT_XP = 25;

        public GameModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userService = serviceProvider.GetService<UserService>();
            _infoService = serviceProvider.GetService<GuildInfoService>();
            _gpt2Service = serviceProvider.GetService<GPT2Service>();
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

        [Command("gpt2"), Summary(@"Submits some text to the GPT2 model")]
        [GuildExclusive(435545282760146944)] // Reality Enclave
        public async Task Gpt2SubmitAsync(string prefix)
        {
            await _gpt2Service.QueueRequestAsync(Context.Message, prefix);
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
