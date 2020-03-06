using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Services;

namespace MorkoBotRavenEdition.Modules
{
    [Summary("Bot Module")]
    [Description("For operator use only.")]
    [Group("bot")]
    class BotModule : ModuleBase
    {
        [Command("invoke"), Summary(@"Invokes a bot URI (Uniform Resource Identifier)")]
        public async Task InvokeAsync(string uri, string value)
        {
            return;
        }

        [Command("shutdown"), Summary(@"Shuts down the bot.")]
        [PermitOwner]
        public async Task ShutdownAsync()
        {
            Process.GetCurrentProcess().Kill();
        }

        protected BotModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
