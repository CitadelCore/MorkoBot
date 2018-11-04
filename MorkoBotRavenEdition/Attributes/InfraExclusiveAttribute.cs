using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Attributes
{
    class InfraExclusiveAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild.Id == 291497857725366272 || context.Guild.Id == 438828266644701214)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("This server is not INFRA-related.");
        }
    }
}
