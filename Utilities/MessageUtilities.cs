using Discord;
using Discord.Net;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MorkoBotRavenEdition.Utilities
{
    internal class MessageUtilities
    {
        private static Random random = new Random();

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

        public static long RandomMessageId() {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand);
        }

        public static async Task HandleCommandFailure(IUserMessage message, IResult result, IUser target = null) {
            await message.AddReactionAsync(new Emoji("❎"));
            var userPm = new EmbedBuilder();

            if (target != null) {
                userPm.WithFooter($"Impersonating {target.Username}#{target.Discriminator}");
            }

            switch(result.Error) {
                case CommandError.Exception:
                    if (result is ExecuteResult exec) {
                        var errorId = Guid.NewGuid();

                        var errStr = $@"eID Ref# {errorId}";
                        userPm.WithFooter(errStr);

                        MorkoBot.Logger.LogError($@"Message ID {message.Id} exception with eID Ref# {errorId}");
                        MorkoBot.Logger.LogError(exec.Exception.ToString());
                    }

                    userPm.WithTitle(@"Internal Exception");
                    userPm.WithDescription("Please check server logs for a stack trace.");
                    userPm.WithColor(Color.Red);
                    break;
                default:
                    userPm.WithTitle(@"Command failure");
                    userPm.WithDescription(result.ErrorReason);
                    userPm.WithColor(Color.Orange);
                    break;
            }

            MorkoBot.Logger.LogTrace($@"Message ID {message.Id} encountered an execution failure: {result.ErrorReason}");
            await message.Channel.SendMessageAsync(string.Empty, false, userPm.Build());
        }
    }
}
