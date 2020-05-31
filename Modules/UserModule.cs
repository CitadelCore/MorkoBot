using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MorkoBotRavenEdition.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static MorkoBotRavenEdition.Models.User.VanityRole;

namespace MorkoBotRavenEdition.Modules
{
    [Summary("User Module")]
    [Description("Manage your server's users!")]
    internal class UserModule : ModuleBase
    {
        private readonly UserService _userService;
        private readonly GuildInfoService _infoService;

        public UserModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userService = serviceProvider.GetService<UserService>();
            _infoService = serviceProvider.GetService<GuildInfoService>();
        }

        [Command("addrole"), Summary(@"Adds a role to yourself, or if you're an admin or moderator, someone else.")]
        public async Task AddRoleAsync([Summary(@"The role to add.")] IRole role, [Summary(@"The user to add a role to (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, ServiceProvider, "Discord Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);
            }

            var vrole = _infoService.GetRole(role.Id, Context.Guild.Id);

            if (vrole == null)
                throw new Exception(@"The role does not exist.");

            if (!(await UserService.DoesUserHaveAnyRole(Context, ServiceProvider, "Discord Moderator")))
            {
                switch (vrole.RestrictionLevel)
                {
                    case RoleRestrictionLevel.ManualOnly:
                        throw new Exception(@"This role is not available for user addition.");
                    case RoleRestrictionLevel.RequestOnly:
                        throw new Exception(@"This role is only available via request, but this feature is not yet implemented.");
                    case RoleRestrictionLevel.Unrestricted:
                        await ((SocketGuildUser) user).AddRoleAsync(role);
                        break;
                    default:
                        throw new InvalidOperationException(@"Invalid restriction level.");
                }
            }
            else
            {
                await ((SocketGuildUser) user).AddRoleAsync(role);
            }
        }

        [Command("removerole"), Summary(@"Removes a role from yourself, or if you're an admin or moderator, someone else.")]
        public async Task RemoveRoleAsync([Summary(@"The role to remove.")] IRole role, [Summary(@"The user to remove a role from (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, ServiceProvider, "Discord Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);
            }

            var vrole = _infoService.GetRole(role.Id, Context.Guild.Id);

            if (vrole == null)
            {
                await SendStatusAsync(@"Error: The role does not exist.", Color.Orange);
                return;
            }

            if (!(await UserService.DoesUserHaveAnyRole(Context, ServiceProvider, "Discord Moderator")) && vrole.RestrictionLevel == RoleRestrictionLevel.ManualOnly)
                throw new Exception(@"This role cannot be manually removed. Please contact a member of staff.");

            await ((SocketGuildUser) user).RemoveRoleAsync(role);
        }

        [Command("getroles"), Summary(@"Gets a list of roles that you can add to yourself or others.")]
        public async Task GetRolesAsync()
        {
            var roleBuilder = new StringBuilder();
            var roles = _infoService.GetAllRoles(Context.Guild.Id);

            foreach (var role in roles)
            {
                string restriction;
                switch (role.RestrictionLevel)
                {
                    case RoleRestrictionLevel.ManualOnly:
                        restriction = @"Manual Only";
                        break;
                    case RoleRestrictionLevel.RequestOnly:
                        restriction = @"Request Only";
                        break;
                    case RoleRestrictionLevel.Unrestricted:
                        restriction = @"Unrestricted";
                        break;
                    default:
                        throw new InvalidOperationException(@"Invalid restriction level.");
                }

                roleBuilder.AppendLine($"Role \"{role.Name}\" ({restriction})");
            }

            var userPm = new EmbedBuilder();
            userPm.WithTitle(@"Vanity Role List");
            userPm.WithDescription(@"Showing a list of vanity roles on this server.");
            userPm.AddField(@"Roles", roleBuilder.ToString());
            userPm.WithColor(Color.Green);

            await ReplyAsync(string.Empty, false, userPm.Build());
        }

        [Command("whois"), Summary(@"Retrieves information about yourself or the specified user. More information is returned depending on your privilege level.")]
        public async Task WhoisAsync([Summary(@"The user to get information about (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            if (!(user is SocketGuildUser guildUser))
                throw new InvalidOperationException(@"User was null.");

            var isStaff = await UserService.DoesUserHaveAnyRole(Context, ServiceProvider, "Discord Moderator", "Discord Admin");
            var isAdmin = await UserService.DoesUserHaveAnyRole(Context, ServiceProvider, "Discord Admin");

            // Extended profile information
            var profile = await _userService.GetProfile(user.Id, Context.Guild.Id);

            var userPm = new EmbedBuilder();
            userPm.WithTitle(@"User Whois Request");

            userPm.WithDescription($"Returning data about user \"{user.Username}\".");
            userPm.WithColor(Color.Green);
            userPm.WithThumbnailUrl(guildUser.GetAvatarUrl());

            userPm.AddField(@"Discord ID", guildUser.Id);
            userPm.AddField(@"Date of creation", guildUser.CreatedAt);
            userPm.AddField(@"Date joined guild", guildUser.JoinedAt);
            userPm.AddField(@"Total increments", profile.IncrementCount);

            userPm.AddField("<:olut:329889326051753986> Experience", $@"{profile.Experience}/{profile.ExperienceTarget} XP", true);
            userPm.AddField("<:geocache:357917503894061059> Current Level", $@"Level {profile.ExperienceLevels}", true);
            //userPm.AddField("<:eeg:359363156885110794> Health", $@"{profile.Health}/10 HP", true);
            userPm.AddField("<:sewercoin:354606163112755230> Sewer Coins", $@"{profile.OpenSewerTokens} OC", true);

            // Administrative information
            if (isStaff)
            {
                var warnings = _userService.GetWarnings(profile);
                var userWarnings = warnings.ToList();
                var active = userWarnings.Select(w => w.TimeAdded <= (DateTime.Now + TimeSpan.FromDays(w.DaysExpiry))).Count();

                userPm.AddField(@":warning: Active warnings", active);
                if (isAdmin)
                {
                    var expired = userWarnings.Select(w => w.TimeAdded > (DateTime.Now + TimeSpan.FromDays(w.DaysExpiry))).Count();
                    userPm.AddField(@":clock3: Expired warnings", expired);
                }
                else
                {
                    userPm.AddField(@":clock3: Expired warnings", @"Witheld. Contact a Discord Admin.");
                }
            }

            await ReplyAsync(string.Empty, false, userPm.Build());
        }

        [Command("warnings"), Summary(@"Retrieves a list of warnings on a user, or yourself.")]
        public async Task WarningsAsync([Summary(@"The user to list warnings for (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var guildUser = (SocketGuildUser) user;
            var isStaff = false;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, ServiceProvider, "Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);

                isStaff = true;
            }

            // Return individual warning data.
            var profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            var isAdmin = await UserService.DoesUserHaveAnyRole(Context, ServiceProvider, "Administrator");

            // Allow admins to view their own warnings.
            if (isAdmin)
                isStaff = true;

            var warnings = _userService.GetWarnings(profile);

            var userWarnings = warnings.ToList();
            if (!userWarnings.Any())
            {
                await SendStatusAsync(@"No warnings are registered on this user.", Color.Orange);
                return;
            }

            // User information block.
            var userPm = new EmbedBuilder();
            userPm.WithTitle(@"User Warnings");

            userPm.WithDescription($"Returning data about user \"{user.Username}\".");
            userPm.WithColor(Color.Green);
            userPm.WithThumbnailUrl(guildUser.GetAvatarUrl());

            await ReplyAsync(string.Empty, false, userPm.Build());

            foreach (var warning in userWarnings)
            {
                var hasExpired = (warning.TimeAdded + TimeSpan.FromDays(warning.DaysExpiry) <= DateTime.Now);

                if (!isAdmin && hasExpired)
                    continue;

                userPm.WithTitle(@"Warning information");
                userPm.WithDescription($"Warning/infraction information for user \"{user.Username}\".");
                userPm.WithColor(Color.Green);

                userPm.AddField(@"Reason", warning.Reason);

                if (!isStaff)
                    userPm.AddField(@"Days to expiry", ((warning.TimeAdded + TimeSpan.FromDays(warning.DaysExpiry)) - DateTime.Now).Days);

                if (isStaff)
                {
                    userPm.AddField(@"Staff member", (await Context.Guild.GetUserAsync((ulong) warning.StaffId)).Username);
                    userPm.AddField(@"Date added", warning.TimeAdded);
                    userPm.AddField(@"Warning length", $@"{warning.DaysExpiry} days");
                }

                await ReplyAsync(string.Empty, false, userPm.Build());
            }
        }
    }
}
