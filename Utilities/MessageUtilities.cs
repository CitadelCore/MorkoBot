﻿using Discord;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Utilities
{
    internal class MessageUtilities
    {
        /// <summary>
        /// Sends a PM (private message) to a user safely.
        /// This is because an error will occur if the user has PMs disabled for non-friends,
        /// and we want to catch this exception and send a notification somewhere else.
        /// </summary>
        public static async Task SendPmSafely(IUser user, IMessageChannel fallback, string text, bool isTts = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                await user.SendMessageAsync(text, isTts, embed, options);
            }
            catch (HttpException)
            {
                if (fallback != null)
                    await fallback.SendMessageAsync(text, isTts, embed, options);
            }
        }
    }
}