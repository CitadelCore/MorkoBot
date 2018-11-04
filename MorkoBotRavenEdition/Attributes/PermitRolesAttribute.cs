using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MorkoBotRavenEdition.Attributes
{
    class PermitRolesAttribute : PreconditionAttribute
    {
        public IList<string> Values { get; set; }

        public PermitRolesAttribute(params string[] values)
        {
            Values = values;
        }

        /// <summary>
        /// Roles that can use these commands regardless of per
        /// </summary>
        private IList<string> OverrideRoles = new List<string>()
        {
            "Global Admin",
            "Loiste Staff",
            "Discord Admin",
        };

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            IEnumerable<string> FinalRoles = OverrideRoles;

            if (Values != null)
                FinalRoles = FinalRoles.Concat(Values);

            ulong ownerId = (await services.GetService<DiscordSocketClient>().GetApplicationInfoAsync()).Owner.Id;

            // Bypass checks if user is bot owner or they have guild administrator permissions
            if (context.User.Id == ownerId || ((SocketGuildUser)context.User).GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();

            // Check user roles against final role list
            if (((SocketGuildUser)context.User).Roles.Where(r => FinalRoles.Contains(r.Name) == true).Any())
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Unable to authenticate. You do not have permission to use this command.");
        }
    }
}
