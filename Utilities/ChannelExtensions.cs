using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace MorkoBotRavenEdition.Utilities
{
    internal static class ChannelExtensions
    {
        public static EmbedBuilder GetResponseEmbed(string title, string info, Color? color = null)
        {
            if (color == null)
                color = Color.Green;

            var builder = new EmbedBuilder
            {
                Title = title,
                Description = info,
                Color = color,
            };

            return builder;
        }

        public static async Task SendStatusAsync(this IMessageChannel channel, string title, string info, Color? color = null) {
            await channel.SendMessageAsync(string.Empty, false, GetResponseEmbed(title, info, color).Build());
        }
    }
}
