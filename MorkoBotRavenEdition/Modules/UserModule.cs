using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MorkoBotRavenEdition.Models.VanityRole;

namespace MorkoBotRavenEdition.Modules
{
    [Group("user")]
    class UserModule : MorkoModuleBase
    {
        private UserService _userService;
        private GuildInfoService _infoService;
        private IServiceProvider _services;

        public UserModule(IServiceProvider services, UserService userService, GuildInfoService infoService)
        {
            _userService = userService;
            _infoService = infoService;
            _services = services;
        }

        [Command("addrole"), Summary("Adds a role to yourself, or if you're an admin or moderator, someone else.")]
        public async Task AddRoleAsync([Summary("The role to add.")] IRole role, [Summary("The user to add a role to (optional).")] IUser user = null)
        {
            ExtendedGuildInfo guild = await _infoService.GetGuildInfo(Context.Guild.Id);

            if (user == null)
                user = Context.User;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, _services, "Discord Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);
            }

            VanityRole vrole = _infoService.GetRole(role.Id, Context.Guild.Id);

            if (vrole == null)
                throw new Exception("The role does not exist.");

            if (!(await UserService.DoesUserHaveAnyRole(Context, _services, "Discord Moderator")))
            {
                switch (vrole.RestrictionLevel)
                {
                    case RoleRestrictionLevel.ManualOnly:
                        throw new Exception("This role is not available for user addition.");
                    case RoleRestrictionLevel.RequestOnly:
                        throw new Exception("This role is only available via request, but this feature is not yet implemented.");
                    case RoleRestrictionLevel.Unrestricted:
                        await (user as SocketGuildUser).AddRoleAsync(role);
                        await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully added the role \"{0}\" to the user.", role.Name), Color.Green).Build());
                        break;
                }
            }
            else
            {
                await (user as SocketGuildUser).AddRoleAsync(role);
                await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully added the role \"{0}\" to the user.", role.Name), Color.Green).Build());
            }
        }

        [Command("removerole"), Summary("Removes a role from yourself, or if you're an admin or moderator, someone else.")]
        public async Task RemoveRoleAsync([Summary("The role to remove.")] IRole role, [Summary("The user to remove a role from (optional).")] IUser user = null)
        {
            ExtendedGuildInfo guild = await _infoService.GetGuildInfo(Context.Guild.Id);

            if (user == null)
                user = Context.User;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, _services, "Discord Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);
            }

            VanityRole vrole = _infoService.GetRole(role.Id, Context.Guild.Id);

            if (vrole == null)
            {
                await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("Error: The role does not exist.", Color.Orange).Build());
                return;
            }

            if (!(await UserService.DoesUserHaveAnyRole(Context, _services, "Discord Moderator")) && vrole.RestrictionLevel == RoleRestrictionLevel.ManualOnly)
                throw new Exception("This role cannot be manually removed. Please contact a member of staff.");

            await (user as SocketGuildUser).RemoveRoleAsync(role);
            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully removed the role \"{0}\" from the user.", role.Name), Color.Green).Build());
        }

        [Command("getroles"), Summary("Gets a list of roles that you can add to yourself or others.")]
        public async Task GetRolesAsync()
        {
            StringBuilder roleBuilder = new StringBuilder();
            IEnumerable<VanityRole> roles = _infoService.GetAllRoles(Context.Guild.Id);

            foreach (VanityRole role in roles)
            {
                string restriction = null;
                switch (role.RestrictionLevel)
                {
                    case RoleRestrictionLevel.ManualOnly:
                        restriction = "Manual Only";
                        break;
                    case RoleRestrictionLevel.RequestOnly:
                        restriction = "Request Only";
                        break;
                    case RoleRestrictionLevel.Unrestricted:
                        restriction = "Unrestricted";
                        break;
                }

                roleBuilder.AppendLine(String.Format("Role \"{0}\" ({1})", role.Name, restriction));
            }

            EmbedBuilder userPm = new EmbedBuilder();
            userPm.WithTitle("Vanity Role List");
            userPm.WithDescription("Showing a list of vanity roles on this server.");
            userPm.AddField("Roles", roleBuilder.ToString());
            userPm.WithColor(Color.Green);

            await Context.User.SendMessageAsync(String.Empty, false, userPm.Build());
        }

        [Command("whois"), Summary("Retrieves information about yourself or the specified user. More information is returned depending on your privilege level.")]
        public async Task WhoisAsync([Summary("The user to get information about (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            SocketGuildUser guildUser = user as SocketGuildUser;

            bool isStaff = await UserService.DoesUserHaveAnyRole(Context, _services, "Discord Moderator", "Discord Admin");
            bool isAdmin = await UserService.DoesUserHaveAnyRole(Context, _services, "Discord Admin");

            // Extended profile information
            UserProfile profile = await _userService.GetProfile(user.Id, Context.Guild.Id);

            EmbedBuilder userPm = new EmbedBuilder();
            userPm.WithTitle("User Whois Request");

            userPm.WithDescription(String.Format("Returning data about user \"{0}\".", user.Username));
            userPm.WithColor(Color.Green);
            userPm.WithThumbnailUrl(guildUser.GetAvatarUrl());

            userPm.AddField("Discord ID", guildUser.Id);
            userPm.AddField("Date of creation", guildUser.CreatedAt);
            userPm.AddField("Date joined guild", guildUser.JoinedAt);
            userPm.AddField("Total increments", profile.IncrementCount);

            userPm.AddField("<:olut:329889326051753986> Experience", String.Format("{0}/{1} XP", profile.Experience, profile.ExperienceTarget), true);
            userPm.AddField("<:geocache:357917503894061059> Current Level", String.Format("Level {0}", profile.ExperienceLevels), true);
            userPm.AddField("<:eeg:359363156885110794> Health", String.Format("{0}/10 HP", profile.Health), true);
            userPm.AddField("<:sewercoin:354606163112755230> Sewer Coins", String.Format("{0} OC", profile.OpenSewerTokens), true);

            // Administrative information
            if (isStaff)
            {
                IEnumerable<UserWarning> warnings = _userService.GetWarnings(profile);
                int active = warnings.Select(w => (w.TimeAdded + TimeSpan.FromDays(w.DaysExpiry) > DateTime.Now)).Count();

                userPm.AddField(":warning: Active warnings", active);
                if (isAdmin)
                {
                    int expired = warnings.Select(w => (w.TimeAdded + TimeSpan.FromDays(w.DaysExpiry) <= DateTime.Now)).Count();
                    userPm.AddField(":clock3: Expired warnings", expired);
                }
                else
                {
                    userPm.AddField(":clock3: Expired warnings", "Witheld. Contact a Discord Admin.");
                }
            }

            await Context.User.SendMessageAsync(String.Empty, false, userPm.Build());
        }

        [Command("warnings"), Summary("Retrieves a list of warnings on a user, or yourself.")]
        public async Task WarningsAsync([Summary("The user to list warnings for (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            SocketGuildUser guildUser = user as SocketGuildUser;
            bool isStaff = false;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, _services, "Discord Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);

                isStaff = true;
            }

            // Return individual warning data.
            UserProfile profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            bool isAdmin = await UserService.DoesUserHaveAnyRole(Context, _services, "Discord Admin");

            // Allow admins to view their own warnings.
            if (isAdmin)
                isStaff = true;

            IEnumerable<UserWarning> warnings = _userService.GetWarnings(profile);

            if (warnings.Count() < 1)
            {
                await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("No warnings are registered on this user.", Color.Orange).Build());
                return;
            }

            // User information block.
            EmbedBuilder userPm = new EmbedBuilder();
            userPm.WithTitle("User Warnings");

            userPm.WithDescription(String.Format("Returning data about user \"{0}\".", user.Username));
            userPm.WithColor(Color.Green);
            userPm.WithThumbnailUrl(guildUser.GetAvatarUrl());

            await Context.User.SendMessageAsync(String.Empty, false, userPm.Build());

            foreach (UserWarning warning in warnings)
            {
                bool hasExpired = (warning.TimeAdded + TimeSpan.FromDays(warning.DaysExpiry) <= DateTime.Now);

                if (!isAdmin && hasExpired)
                    continue;

                EmbedBuilder warningPm = new EmbedBuilder();
                userPm.WithTitle("Warning information");
                userPm.WithDescription(String.Format("Warning/infraction information for user \"{0}\".", user.Username));
                userPm.WithColor(Color.Green);

                userPm.AddField("Reason", warning.Reason);

                if (!isStaff)
                    userPm.AddField("Days to expiry", ((warning.TimeAdded + TimeSpan.FromDays(warning.DaysExpiry)) - DateTime.Now).Days);

                if (isStaff)
                {
                    userPm.AddField("Staff member", (await Context.Guild.GetUserAsync(warning.StaffId)).Username);
                    userPm.AddField("Date added", warning.TimeAdded);
                    userPm.AddField("Warning length", String.Format("{0} days", warning.DaysExpiry));
                }

                await Context.User.SendMessageAsync(String.Empty, false, userPm.Build());
            }
        }
    }
}
