using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Attributes;
using Discord.WebSocket;
using System.Linq;

namespace MorkoBotRavenEdition.Modules
{
    [Summary("Server Integrity Manager")]
    [Group("admin")]
    class AdminModule : MorkoModuleBase
    {
        private UserService _userService;
        public AdminModule(UserService userService)
        {
            _userService = userService;
        }

        [Command("warn"), Summary("Submits a disciplinary warning to a user's profile.")]
        [PermitRoles("Discord Moderator")]
        public async Task WarnAsync([Summary("The user to warn.")] IUser user, [Summary("Warn reason to be logged.")] string reason)
        {
            UserProfile profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            await _userService.AddWarning(profile, new UserWarning()
            {
                DaysExpiry = 30,
                Reason = reason,
                StaffId = Context.User.Id,
            });
        }

        [Command("kick"), Summary("Kicks a user from the server, with an optional reason to be logged.")]
        [PermitRoles("Discord Moderator")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([Summary("The user to kick.")] IUser user, [Summary("Optional kick reason.")] string reason = null)
        {
            await _userService.ThrowIfCannotModify(Context.User, user);

            // Build the user response embed
            EmbedBuilder userPm = GetResponseEmbed(String.Format("You've been kicked from the server {0}. For more information, please contact a staff member.", Context.Guild.Name), Color.Red);

            // Send the PM and kick the user
            await user.SendMessageAsync(String.Empty, false, userPm.Build());
            await ((IGuildUser)user).KickAsync(reason);

            // Send the result message
            EmbedBuilder adminPm = GetResponseEmbed(String.Format("<:banboot:418448078031290369> Successfully kicked the user {0} from the server. This action has been logged.", user.Username), Color.Green);

            await ReplyAsync(String.Empty, false, adminPm.Build());
        }

        [Command("ban"), Summary("Bans a user from the server, with an optional prune duration and reason to be logged. If the duration is not specified, the ban will be permanent.")]
        [PermitRoles("Discord Moderator")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync([Summary("The user to ban.")] IUser user, [Summary("Days from which the user's messages should be pruned.")] int pruneDays = 0, [Summary("The ban reason to be logged. Optional.")] string reason = null)
        {
            await _userService.ThrowIfCannotModify(Context.User, user);

            // Build the user PM message
            EmbedBuilder userPm = GetResponseEmbed(String.Format("You've been banned from the server {0}. For more information, please contact a staff member.", Context.Guild.Name), Color.Red);

            // Send the PM and ban the user
            await user.SendMessageAsync(String.Empty, false, userPm.Build());
            await Context.Guild.AddBanAsync(user, pruneDays, reason);

            // Send the result message
            EmbedBuilder adminPm = GetResponseEmbed(String.Format("<:banboot:418448078031290369> Successfully banned the user {0} from the server. This action has been logged.", user.Username), Color.Green);

            await ReplyAsync(String.Empty, false, adminPm.Build());
        }

        [Command("resetprofile"), Summary("Resets a user's profile, including their XP, levels, and other information. Use with caution.")]
        [PermitRoles()]
        public async Task ResetProfileAsync([Summary("The user to reset.")] IUser user, ulong guild)
        {
            await _userService.DeleteProfile(user.Id, guild);
            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("Successfully cleared the user profile.", Color.Green).Build());
        }

        [Command("resetpardon"), Summary("Purges a user's warnings, infraction points and disciplinary status.")]
        [PermitRoles()]
        public async Task ResetPardonAsync([Summary("The user to reset.")] IUser user)
        {
            UserProfile profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            await _userService.ResetWarnings(profile);
        }

        [Command("addxp"), Summary("Adds a specified amount of XP to the user. If sufficiently large, the user will level up.")]
        [PermitRoles()]
        public async Task AddExperienceAsync([Summary("The user to modify.")] IUser user, [Summary("The amount of experience to add to the user.")] int xp)
        {
            UserProfile profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            await _userService.AddExperience(profile, xp);

            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully added {0} XP to the user. The XP amount is now {1}.", xp, profile.Experience), Color.Green).Build());
        }

        [Command("setcoins"), Summary("Sets the number of OC (Sewer Coins) on a user.")]
        [PermitRoles()]
        public async Task SetCoinsAsync([Summary("The user to modify.")] IUser user, [Summary("The amount of coins the user should have.")] int oc)
        {
            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("Successfully set the user's coins.", Color.Green).Build());
        }

        [Command("sethealth"), Summary("Sets the user's health. Setting the health to 0 will kill them. -1 will make them immortal.")]
        [PermitRoles()]
        public async Task SetHealthAsync([Summary("The user to modify.")] IUser user, [Summary("The amount of health the user should have..")] int health)
        {
            if (health != -1 && (health < 0 || health > 10))
                throw new ArgumentOutOfRangeException("Health is out of range. Health must be 0-10, or -1.");

            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed("Successfully set the user's health.", Color.Green).Build());
        }
    }
}
