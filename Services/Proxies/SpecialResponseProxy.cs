using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MorkoBotRavenEdition.Utilities;

namespace MorkoBotRavenEdition.Services.Proxies
{
    internal sealed class SpecialResponseProxy : ResponseProxy
    {
        public SpecialResponseProxy()
        {
            ProxyType = ResponseProxyType.MessageOnly;
        }

        internal override async Task Run(DiscordSocketClient client, SocketUserMessage message)
        {
            // these will only work on prod Loiste server
            if (((SocketGuildChannel) message.Channel).Guild.Id != GuildConstants.LOISTE_PROD) return;
            var content = message.Content.ToLower();

            if (content.Contains("morko") || message.Content.ToLower().Contains("mörkö"))
                await message.AddReactionAsync(Emote.Parse("<:morko:329887947736350720>"));

            if (content.Contains("raven"))
                await message.AddReactionAsync(Emote.Parse("<:raven:354971314877890560>"));

            if (content.Contains("perkele"))
                await message.AddReactionAsync(Emote.Parse("<:perkele:374644476800401408>"));
        }
    }
}
