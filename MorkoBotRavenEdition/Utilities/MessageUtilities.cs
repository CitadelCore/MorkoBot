using Discord;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Utilities
{
    class MessageUtilities
    {
        /// <summary>
        /// Sends a PM (private message) to a user safely.
        /// This is because an error will occur if the user has PMs disabled for non-friends,
        /// and we want to catch this exception and send a notification somewhere else.
        /// </summary>
        public static async Task SendPMSafely(IUser user, IMessageChannel fallback, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                await user.SendMessageAsync(text, isTTS, embed, options);
            }
            catch (HttpException)
            {
                if (fallback != null)
                    await fallback.SendMessageAsync(text, isTTS, embed, options);
            }
        }
    }
}
