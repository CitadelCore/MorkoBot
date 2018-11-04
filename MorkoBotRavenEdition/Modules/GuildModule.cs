using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static MorkoBotRavenEdition.Models.VanityRole;

namespace MorkoBotRavenEdition.Modules
{
    /// <summary>
    /// Responsible for high-level guild management administrative tasks.
    /// </summary>
    [Group("guild")]
    class GuildModule : MorkoModuleBase
    {
        private GuildInfoService _infoService;
        public GuildModule(GuildInfoService infoService)
        {
            _infoService = infoService;
        }

        [Command("addrole"), Summary("Adds a user/vanity role to the server role list.")]
        [PermitRoles()]
        public async Task AddRoleAsync([Summary("The role to add to the role list.")] IRole role, [Summary("The restriction level. Defaults to ManualOnly.")] RoleRestrictionLevel restrictionLevel = RoleRestrictionLevel.ManualOnly)
        {
            // Role prerequisites
            if (role.IsManaged)
                throw new Exception("Cannot add Discord-managed roles.");

            VanityRole vrole = _infoService.GetRole(role.Id, Context.Guild.Id);

            // Check for existing roles
            if (vrole != null)
            {
                await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("Error: The role already exists. Delete it, or use !guild updaterole to update it.", Color.Orange).Build());
                return;
            }

            await _infoService.AddVanityRole(role.Id, Context.Guild.Id, role.Name, restrictionLevel);
            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully added the role \"{0}\".", role), Color.Green).Build());
        }

        [Command("removerole"), Summary("Removes a user/vanity role from the server role list.")]
        [PermitRoles()]
        public async Task RemoveRoleAsync([Summary("The role to remove from the server role list.")] IRole role)
        {
            // Ensure role exists
            if (_infoService.GetRole(role.Id, Context.Guild.Id) == null)
            {
                await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("Error: The role does not exist.", Color.Orange).Build());
                return;
            }

            await _infoService.RemoveVanityRole(role.Id, Context.Guild.Id);
            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully removed the role \"{0}\".", role), Color.Green).Build());
        }

        [Command("updaterole"), Summary("Updates a user/vanity role with a new restriction level.")]
        [PermitRoles()]
        public async Task UpdateRoleAsync([Summary("The role to update.")] IRole role, [Summary("The new restriction level.")] RoleRestrictionLevel restrictionLevel)
        {
            // Ensure role exists
            if (_infoService.GetRole(role.Id, Context.Guild.Id) == null)
            {
                await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("Error: The role does not exist.", Color.Orange).Build());
                return;
            }

            await _infoService.UpdateVanityRole(role.Id, Context.Guild.Id, restrictionLevel);
            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully updated the role \"{0}\".", role), Color.Green).Build());
        }
    }
}
