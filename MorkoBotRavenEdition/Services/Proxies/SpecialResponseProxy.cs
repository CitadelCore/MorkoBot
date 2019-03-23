using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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
            if (((SocketGuildChannel) message.Channel).Guild.Id != 291497857725366272) return;

            if (message.Content.ToLower().Contains("morko") || message.Content.ToLower().Contains("mörkö"))
                await message.AddReactionAsync(Emote.Parse("<:morko:329887947736350720>"));

            if (message.Content.ToLower().Contains("raven"))
                await message.AddReactionAsync(Emote.Parse("<:raven:354971314877890560>"));

            if (message.Content.ToLower().Contains("nullifactor"))
                await message.AddReactionAsync(Emote.Parse("<:5pm:423182998343516170>"));

            if (message.Content.ToLower().Contains("perkele"))
                await message.AddReactionAsync(Emote.Parse("<:perkele:374644476800401408>"));

            if (message.Content.ToLower().Contains("oh god") || message.Content.ToLower().Contains("ohgodno"))
                await message.AddReactionAsync(Emote.Parse("<:ohgodno:374303106961375242>"));
        }
    }
}
