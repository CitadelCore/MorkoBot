using Discord;
using Discord.Commands;

namespace MorkoBotRavenEdition.Models.Discord
{
    public class SudoCommandContext : ICommandContext
    {
        public SudoCommandContext(ICommandContext parent, IUser user, IUserMessage message) {
            Client = parent.Client;
            Guild = parent.Guild;
            Channel = parent.Channel;
            User = user;
            Message = message;

            if (parent is CommandContext ctx) {
                IsPrivate = ctx.IsPrivate;
            }
        }

        //
        public IDiscordClient Client { get; }
        //
        public IGuild Guild { get; }
        //
        public IMessageChannel Channel { get; }
        //
        public IUser User { get; }
        //
        public IUserMessage Message { get; }
        //
        // Summary:
        //     Indicates whether the channel that the command is executed in is a private channel.
        public bool IsPrivate { get; }
    }
}