using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Models.User;

namespace MorkoBotRavenEdition.Modules
{
    [Summary(@"Server Integrity Manager")]
    [Description("Moderate your server with style.")]
    [Group("admin")]
    internal class AdminModule : ModuleBase
    {
        private readonly UserService _userService;
        private readonly BanTrackerService _banTrackerService;
        public AdminModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userService = serviceProvider.GetService<UserService>();
            _banTrackerService = serviceProvider.GetService<BanTrackerService>();
        }

        [Command("warn"), Summary(@"Submits a disciplinary warning to a user's profile.")]
        [PermitRoles("Discord Moderator")]
        public async Task WarnAsync([Summary(@"The user to warn.")] IUser user, [Summary(@"Warn reason to be logged.")] string reason)
        {
            var profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            await _userService.AddWarning(profile, new UserWarning
            {
                DaysExpiry = 30,
                Reason = reason,
                StaffId = (long) Context.User.Id,
            });
        }

        [Command("kick"), Summary(@"Kicks a user from the server, with an optional reason to be logged.")]
        [PermitRoles("Discord Moderator")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([Summary(@"The user to kick.")] IUser user, [Summary(@"Optional kick reason.")] string reason = null)
        {
            await _userService.ThrowIfCannotModify(Context.User, user);

            // Build the user response embed
            var userPm = GetResponseEmbed($@"You've been kicked from the server {Context.Guild.Name}. For more information, please contact a staff member.", Color.Red);

            // Send the PM and kick the user
            await user.SendMessageAsync(string.Empty, false, userPm.Build());
            await ((IGuildUser)user).KickAsync(reason);

            // Send the result message
            var adminPm = GetResponseEmbed($@"<:banboot:418448078031290369> Successfully kicked the user {user.Username} from the server. This action has been logged.", Color.Green);

            await ReplyAsync(string.Empty, false, adminPm.Build());
        }

        [Command("ban"), Summary(@"Bans a user from the server, with an optional ban duration, prune duration and reason to be logged. If the duration is not specified, the ban will be permanent.")]
        [PermitRoles("Discord Moderator")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync([Summary(@"The user to ban.")] IUser user, [Summary(@"Hours until the ban expires. 0 to never expire.")] int banHours = 0, [Summary(@"Days from which the user's messages should be pruned.")] int pruneDays = 0, [Summary(@"The ban reason to be logged. Optional.")] string reason = null)
        {
            await _userService.ThrowIfCannotModify(Context.User, user);

            // Build the user PM message
            var userPm = GetResponseEmbed($@"Your membership of {Context.Guild.Name} has been temporarily suspended. For more information, please contact a staff member.", Color.Red);

            // Send the PM and ban the user
            await user.SendMessageAsync(string.Empty, false, userPm.Build());
            await Context.Guild.AddBanAsync(user, pruneDays, reason);

            // Add the ban timer
            if (banHours != 0)
                _banTrackerService.StartTrackingBan(Context.Guild, user.Id, banHours);

            // Send the result message
            var adminPm = GetResponseEmbed($@"<:banboot:418448078031290369> Successfully banned the user {user.Username} from the server. This action has been logged.", Color.Green);

            await ReplyAsync(string.Empty, false, adminPm.Build());
        }

        [Command("unban"), Summary(@"Revokes a user's ban.")]
        [PermitRoles("Discord Moderator")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task UnbanAsync([Summary(@"The user to unban.")] IUser user)
        {
            await _userService.ThrowIfCannotModify(Context.User, user);
            await Context.Guild.RemoveBanAsync(user);
            _banTrackerService.StopTrackingBan(Context.Guild, user.Id);

            var adminPm = GetResponseEmbed($@"<:banboot:418448078031290369> Successfully unbanned the user {user.Username} from the server. This action has been logged.", Color.Green);
            await ReplyAsync(string.Empty, false, adminPm.Build());
        }

        [Command("resetprofile"), Summary(@"Resets a user's profile, including their XP, levels, and other information. Use with caution.")]
        [PermitRoles]
        public async Task ResetProfileAsync([Summary(@"The user to reset.")] IUser user, ulong guild)
        {
            await _userService.DeleteProfile(user.Id, guild);
            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed(@"Successfully cleared the user profile.", Color.Green).Build());
        }

        [Command("resetpardon"), Summary(@"Purges a user's warnings, infraction points and disciplinary status.")]
        [PermitRoles]
        public async Task ResetPardonAsync([Summary(@"The user to reset.")] IUser user)
        {
            var profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            await _userService.ResetWarnings(profile);
        }

        [Command("addxp"), Summary(@"Adds a specified amount of XP to the user. If sufficiently large, the user will level up.")]
        [PermitRoles]
        public async Task AddExperienceAsync([Summary(@"The user to modify.")] IUser user, [Summary(@"The amount of experience to add to the user.")] int xp)
        {
            var profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            await _userService.AddExperience(profile, xp);

            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed($@"Successfully added {xp} XP to the user. The XP amount is now {profile.Experience}.", Color.Green).Build());
        }

        [Command("setcoins"), Summary(@"Sets the number of OC (Sewer Coins) on a user.")]
        [PermitRoles]
        public async Task SetCoinsAsync([Summary(@"The user to modify.")] IUser user, [Summary(@"The amount of coins the user should have.")] int oc)
        {
            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed(@"Successfully set the user's coins.", Color.Green).Build());
        }

        /**
        [Command("sethealth"), Summary(@"Sets the user's health. Setting the health to 0 will kill them. -1 will make them immortal.")]
        [PermitRoles]
        public async Task SetHealthAsync([Summary(@"The user to modify.")] IUser user, [Summary(@"The amount of health the user should have..")] int health)
        {
            if (health != -1 && (health < 0 || health > 10))
                throw new ArgumentOutOfRangeException(nameof(health), "Health is out of range. Health must be 0-10, or -1.");

            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed(@"Successfully set the user's health.", Color.Green).Build());
        }*/
    }
}
