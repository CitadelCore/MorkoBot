using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Attributes
{
    internal class InfraExclusiveAttribute : PreconditionAttribute
    {
        private const ulong ProductionGuildId = 291497857725366272;
        private const ulong DevelopmentGuildId = 438828266644701214;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild.Id == ProductionGuildId || context.Guild.Id == DevelopmentGuildId)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("This server is not INFRA-related.");
        }
    }
}
