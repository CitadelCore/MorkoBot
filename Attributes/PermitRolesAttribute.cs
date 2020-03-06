using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MorkoBotRavenEdition.Attributes
{
    internal class PermitRolesAttribute : PreconditionAttribute
    {
        private IList<string> Values { get; }

        public PermitRolesAttribute(params string[] values)
        {
            Values = values;
        }

        /// <summary>
        /// Roles that can use these commands regardless of per
        /// </summary>
        private readonly IList<string> _overrideRoles = new List<string>()
        {
            "Global Admin",
            "Loiste Staff",
            "Discord Admin",
        };

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            IEnumerable<string> finalRoles = _overrideRoles;

            if (Values != null)
                finalRoles = finalRoles.Concat(Values);

            var ownerId = (await services.GetService<DiscordSocketClient>().GetApplicationInfoAsync()).Owner.Id;

            // Bypass checks if user is bot owner or they have guild administrator permissions
            if (context.User.Id == ownerId || ((SocketGuildUser)context.User).GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();

            // Check user roles against final role list
            return ((SocketGuildUser)context.User).Roles.Any(r => finalRoles.Contains(r.Name))
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError(@"Unable to authenticate. You do not have permission to use this command.");
        }
    }
}
