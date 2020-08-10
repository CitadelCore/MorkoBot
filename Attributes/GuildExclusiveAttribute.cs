using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Attributes
{
    internal class GuildExclusiveAttribute : PreconditionAttribute
    {
        private ulong _id;
        internal GuildExclusiveAttribute(ulong id)
        {
            _id = id;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild.Id == _id)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("This feature is not enabled on this server.");
        }
    }
}
