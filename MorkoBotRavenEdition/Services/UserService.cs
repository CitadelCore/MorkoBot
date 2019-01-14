using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;

namespace MorkoBotRavenEdition.Services
{
    internal class UserService
    {
        private readonly DiscordSocketClient _client;
        private readonly BotDbContext _context;

        public UserService(DiscordSocketClient dsc, BotDbContext dbContext)
        {
            _client = dsc;
            _context = dbContext;
        }

        /// <summary>
        /// Returns a user profile from a Discord unique User ID.
        /// If the profile does not exist, this method will automatically create one.
        /// </summary>
        public async Task<UserProfile> GetProfile(ulong id, ulong guild)
        {
            var profile = _context.UserProfiles.FirstOrDefault(u => u.Identifier == id && u.GuildIdentifier == guild);

            if (profile != null) return profile;

            profile = new UserProfile { Identifier = id, GuildIdentifier = guild, LastIncremented = DateTime.Now - TimeSpan.FromHours(1) };
            _context.Add(profile);
            await _context.SaveChangesAsync();

            return profile;
        }

        /// <summary>
        /// Saves changes in a profile.
        /// </summary>
        public async Task SaveProfile(UserProfile profile)
        {
            _context.Update(profile);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a profile and all associated data.
        /// </summary>
        public async Task DeleteProfile(ulong id, ulong guild)
        {
            var profile = _context.UserProfiles.SingleOrDefault(u => u.Identifier == id && u.GuildIdentifier == guild);

            _context.Remove(profile ?? throw new InvalidOperationException());
            await _context.SaveChangesAsync();
        }

        public async Task AddWarning(UserProfile profile, UserWarning warning)
        {
            var guild = _client.GetGuild(profile.GuildIdentifier);

            warning.UserId = profile.Identifier;
            warning.TimeAdded = DateTime.Now;
            warning.GuildIdentifier = guild.Id;
            _context.Add(warning);

            await _context.SaveChangesAsync();

            var embed = new EmbedBuilder();
            embed.WithTitle(@"Server Integrity Manager");
            embed.WithDescription($@"You've recieved a warning for: {warning.Reason}");
            embed.WithFooter($@"Expires in {warning.DaysExpiry} days.");
            embed.WithColor(Color.Red);

            var adminEmbed = new EmbedBuilder();
            adminEmbed.WithTitle(@"Server Integrity Manager");
            adminEmbed.WithDescription($"The staff member {guild.GetUser(warning.StaffId).Username} has registered a warning for the user {guild.GetUser(warning.UserId).Username} with the reason \"{warning.Reason}\"");
            adminEmbed.WithFooter($@"Expires in {warning.DaysExpiry} days.");
            adminEmbed.WithColor(Color.Red);

            // Attempt to notify the user
            await MessageUtilities.SendPmSafely(_client.GetUser(profile.Identifier), null, string.Empty, false, embed.Build());

            // Notify the staff channel

            // hack
            await ((SocketTextChannel)_client.GetGuild(Convert.ToUInt64(ConfigurationManager.AppSettings.Get("DefaultGuildId"))).GetChannel(Convert.ToUInt64(ConfigurationManager.AppSettings.Get("StaffChannelId"))))
                .SendMessageAsync(string.Empty, false, adminEmbed.Build());
        }

        public IEnumerable<UserWarning> GetWarnings(UserProfile profile)
        {
            return _context.UserWarnings.Where(w => w.UserId == profile.Identifier && w.GuildIdentifier == profile.GuildIdentifier);
        }

        public async Task ResetWarnings(UserProfile profile)
        {
            _context.RemoveRange(_context.UserWarnings.Where(w => w.UserId == profile.Identifier && w.GuildIdentifier == profile.GuildIdentifier));
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a specified amount of XP to a user.
        /// This will level up and send user PMs as necessary.
        /// No PMs will be sent if the user does not have them enabled.
        /// </summary>
        public async Task AddExperience(UserProfile profile, int xp)
        {
            // Add the XP
            profile.Experience += xp;
            
            // Return if the target is not exceeded
            if (profile.Experience <= profile.ExperienceTarget)
                return;

            // Calculate how many levels the added XP could theoretically add
            var levels = 0;
            while (true)
            {
                if (profile.Experience <= profile.ExperienceTarget)
                    break;

                profile.ExperienceTarget = profile.ExperienceTarget * 2;
                levels++;
            }

            if (levels == 0)
                return;

            profile.ExperienceLevels += levels;
            await SaveProfile(profile);

            // Build the embed
            var embed = new EmbedBuilder();
            embed.WithTitle(@"Leveled Up!");
            embed.WithDescription($@"You just leveled up to <:geocache:357917503894061059> Level {profile.ExperienceLevels}.");
            embed.WithColor(Color.Green);

            embed.AddField("<:olut:329889326051753986> Experience", $@"{profile.Experience}/{profile.ExperienceTarget} XP", true);
            embed.AddField("<:geocache:357917503894061059> Current Level", $@"Level {profile.ExperienceLevels}", true);

            // Attempt to notify the user
            await MessageUtilities.SendPmSafely(_client.GetUser(profile.Identifier), null, string.Empty, false, embed.Build());
        }

        /// <summary>
        /// Sets the health of a user, and if it falls below 1,
        /// the user's statistics and inventory will be cleared.
        /// </summary>
        public async Task SetHealth(UserProfile profile, int health)
        {
            if (health == profile.Health)
                return;

            if (health > 0)
            {
                var embed2 = new EmbedBuilder();
                embed2.WithDescription($"Your <:eeg:359363156885110794> Health is now {health}/10 HP!");

                if (health < profile.Health)
                {
                    embed2.WithTitle("Oh perkele, you've lost health!");
                    embed2.WithColor(Color.Orange);
                    await MessageUtilities.SendPmSafely(_client.GetUser(profile.Identifier), null, string.Empty, false, embed2.Build());
                }

                if (health > profile.Health)
                {
                    embed2.WithTitle("Awesome, you've been healed!");
                    embed2.WithColor(Color.Green);
                    await MessageUtilities.SendPmSafely(_client.GetUser(profile.Identifier), null, string.Empty, false, embed2.Build());
                }

                profile.Health = health;
                await SaveProfile(profile);
                return;
            }

            profile.Health = 10;
            profile.OpenSewerTokens = 0;
            await SaveProfile(profile);

            // Build the embed
            var embed = new EmbedBuilder();
            embed.WithTitle("Oh perkele, you've died!");
            embed.WithDescription("Your health has fallen too low and you have died. Game over, sorry. All your stats except for XP and levels have been reset.");
            embed.WithColor(Color.Orange);

            // Attempt to notify the user
            await MessageUtilities.SendPmSafely(_client.GetUser(profile.Identifier), null, string.Empty, false, embed.Build());
        }

        /// <summary>
        /// Checks whether a user can modify another user.
        /// </summary>
        /// <param name="actor">The user initiating the action.</param>
        /// <param name="subject">The user being modified.</param>
        private async Task<bool> CanUserModifyUser(IUser actor, IUser subject)
        {
            var ownerId = (await _client.GetApplicationInfoAsync()).Owner.Id;

            // Bypass checks if user is bot owner or they have guild administrator permissions
            if (actor.Id == ownerId || ((SocketGuildUser)actor).GuildPermissions.Administrator)
                return true;

            return ((SocketGuildUser)actor).Roles.Sum(r => r.Position) > ((SocketGuildUser)subject).Roles.Sum(r => r.Position);
        }

        public static async Task<bool> DoesUserHaveAnyRole(ICommandContext context, IServiceProvider services, params string[] roles)
        {
            var permitRoles = new PermitRolesAttribute(roles);

            return (await permitRoles.CheckPermissionsAsync(context, null, services)).IsSuccess;
        }

        /// <summary>
        /// Throws an exception if the specified actor cannot modify the specified subject.
        /// </summary>
        public async Task ThrowIfCannotModify(IUser actor, IUser subject)
        {
            if (!(await CanUserModifyUser(actor, subject)))
                throw new Exception("Cannot modify this user. Access denied.");
        }

        public static async Task ThrowIfHasNoPermissions(ICommandContext context, IServiceProvider services, params string[] roles)
        {
            if (!(await DoesUserHaveAnyRole(context, services, roles)))
                throw new Exception("You don't have the required permissions to perform this action.");
        }
    }
}
