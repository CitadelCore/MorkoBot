using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Services;
using MorkoBotRavenEdition.Models.Guild;
using MorkoBotRavenEdition.Models.Tasks;
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using static MorkoBotRavenEdition.Models.User.VanityRole;

namespace MorkoBotRavenEdition.Modules
{
    /// <summary>
    /// Responsible for high-level guild management administrative tasks.
    /// </summary>
    [Summary("Guild Manager")]
    [Description("Define your server.")]
    [Group("guild")]
    internal class GuildModule : ModuleBase
    {
        private readonly GuildInfoService _infoService;
        private readonly MessageLoggerService _loggerService;

        public GuildModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _infoService = serviceProvider.GetService<GuildInfoService>();
            _loggerService = serviceProvider.GetService<MessageLoggerService>();
        }

        [Command("addrole"), Summary(@"Adds a user/vanity role to the server role list.")]
        [PermitRoles]
        public async Task AddRoleAsync([Summary(@"The role to add to the role list.")] IRole role, [Summary(@"The restriction level. Defaults to ManualOnly.")] RoleRestrictionLevel restrictionLevel = RoleRestrictionLevel.ManualOnly)
        {
            // Role prerequisites
            if (role.IsManaged)
                throw new Exception(@"Cannot add Discord-managed roles.");

            var vrole = _infoService.GetRole(role.Id, Context.Guild.Id);

            // Check for existing roles
            if (vrole != null)
            {
                await SendStatusAsync(@"Error: The role already exists. Delete it, or use !guild updaterole to update it.", Color.Orange);
                return;
            }

            await _infoService.AddVanityRole(role.Id, Context.Guild.Id, role.Name, restrictionLevel);
            await SendStatusAsync($"Successfully added the role \"{role}\".", Color.Green);
        }

        [Command("removerole"), Summary(@"Removes a user/vanity role from the server role list.")]
        [PermitRoles]
        public async Task RemoveRoleAsync([Summary(@"The role to remove from the server role list.")] IRole role)
        {
            // Ensure role exists
            if (_infoService.GetRole(role.Id, Context.Guild.Id) == null)
            {
                await SendStatusAsync(@"Error: The role does not exist.", Color.Orange);
                return;
            }

            await _infoService.RemoveVanityRole(role.Id, Context.Guild.Id);
            await SendStatusAsync($"Successfully removed the role \"{role}\".", Color.Green);
        }

        [Command("updaterole"), Summary(@"Updates a user/vanity role with a new restriction level.")]
        [PermitRoles]
        public async Task UpdateRoleAsync([Summary(@"The role to update.")] IRole role, [Summary(@"The new restriction level.")] RoleRestrictionLevel restrictionLevel)
        {
            // Ensure role exists
            if (_infoService.GetRole(role.Id, Context.Guild.Id) == null)
            {
                await SendStatusAsync(@"Error: The role does not exist.", Color.Orange);
                return;
            }

            await _infoService.UpdateVanityRole(role.Id, Context.Guild.Id, restrictionLevel);
            await SendStatusAsync($"Successfully updated the role \"{role}\".", Color.Green);
        }

        [Command("logexport"), Summary(@"Exports chat logs for a specified channel and date.")]
        [PermitRoles]
        public async Task LogExportAsync(ITextChannel channel = null, DateTime? start = null, DateTime? end = null)
        {
            var channelId = channel != null ? (long?)channel.Id : null;

            var export = await _loggerService.LogExportAsync(Context.Client, (long)Context.Guild.Id, channelId, start, end);
            if (export == null) {
                await SendStatusAsync(@"Export failed. Please contact a developer.", Color.Red);
                return;
            }

            await SendStatusAsync($@"Export success! Link: {export}", Color.Green);
        }

        [Command("logbackfill"), Summary(@"Backfills the entire message history of a server into MorkoBot's message cache.")]
        [PermitOwner]
        public async Task LogBackfillAsync() {
            _loggerService.RunBackfill(Context);
            await SendStatusAsync(@"Backfill job started; this may take a while, so please wait.", Color.Green);
        }
    }
}
