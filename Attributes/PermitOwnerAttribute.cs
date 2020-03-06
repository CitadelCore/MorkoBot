using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace MorkoBotRavenEdition.Attributes
{
    internal class PermitOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ownerId = (await services.GetService<DiscordSocketClient>().GetApplicationInfoAsync()).Owner.Id;
            return context.User.Id == ownerId ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("You do not have permission to use this command. Restricted to bot operators only.");
        }
    }
}
