using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MorkoBotRavenEdition.Attributes;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Discord.Rest;

namespace MorkoBotRavenEdition.Services
{
    class UserService
    {
        private DiscordSocketClient Client;
        private BotDbContext Context;

        public UserService(DiscordSocketClient dsc, BotDbContext dbContext)
        {
            Client = dsc;
            Context = dbContext;
        }

        /// <summary>
        /// Returns a user profile from a Discord unique User ID.
        /// If the profile does not exist, this method will automatically create one.
        /// </summary>
        public async Task<UserProfile> GetProfile(ulong id, ulong guild)
        {
            UserProfile profile = Context.UserProfiles.Where(u => u.Identifier == id && u.GuildIdentifier == guild).FirstOrDefault();

            if (profile == null)
            {
                profile = new UserProfile() { Identifier = id, GuildIdentifier = guild, LastIncremented = DateTime.Now - TimeSpan.FromHours(1) };
                Context.Add(profile);
                await Context.SaveChangesAsync();
            }

            return profile;
        }

        /// <summary>
        /// Saves changes in a profile.
        /// </summary>
        public async Task SaveProfile(UserProfile profile)
        {
            Context.Update(profile);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a profile and all associated data.
        /// </summary>
        public async Task DeleteProfile(ulong id, ulong guild)
        {
            UserProfile profile = Context.UserProfiles.Where(u => u.Identifier == id && u.GuildIdentifier == guild).SingleOrDefault();

            Context.Remove(profile);
            await Context.SaveChangesAsync();
        }

        public async Task AddWarning(UserProfile profile, UserWarning warning)
        {
            SocketGuild guild = Client.GetGuild(profile.GuildIdentifier);

            warning.UserId = profile.Identifier;
            warning.TimeAdded = DateTime.Now;
            warning.GuildIdentifier = guild.Id;
            Context.Add(warning);

            await Context.SaveChangesAsync();

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Server Integrity Manager");
            embed.WithDescription(String.Format("You've recieved a warning for: {0}", warning.Reason));
            embed.WithFooter(String.Format("Expires in {0} days.", warning.DaysExpiry));
            embed.WithColor(Color.Red);

            EmbedBuilder adminEmbed = new EmbedBuilder();
            adminEmbed.WithTitle("Server Integrity Manager");
            adminEmbed.WithDescription(String.Format("The staff member {0} has registered a warning for the user {1} with the reason \"{2}\"", 
                guild.GetUser(warning.StaffId).Username, guild.GetUser(warning.UserId).Username, warning.Reason));
            adminEmbed.WithFooter(String.Format("Expires in {0} days.", warning.DaysExpiry));
            adminEmbed.WithColor(Color.Red);

            // Attempt to notify the user
            await MessageUtilities.SendPMSafely(Client.GetUser(profile.Identifier), null, String.Empty, false, embed.Build());

            // Notify the staff channel

            // hack
            await ((SocketTextChannel)Client.GetGuild(Convert.ToUInt64(ConfigurationManager.AppSettings.Get("DefaultGuildId"))).GetChannel(Convert.ToUInt64(ConfigurationManager.AppSettings.Get("StaffChannelId"))))
                .SendMessageAsync(String.Empty, false, adminEmbed.Build());
        }

        public IEnumerable<UserWarning> GetWarnings(UserProfile profile)
        {
            return Context.UserWarnings.Where(w => w.UserId == profile.Identifier && w.GuildIdentifier == profile.GuildIdentifier);
        }

        public async Task ResetWarnings(UserProfile profile)
        {
            Context.RemoveRange(Context.UserWarnings.Where(w => w.UserId == profile.Identifier && w.GuildIdentifier == profile.GuildIdentifier));
            await Context.SaveChangesAsync();
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
            int levels = 0;
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
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Leveled Up!");
            embed.WithDescription(String.Format("You just leveled up to <:geocache:357917503894061059> Level {0}.", profile.ExperienceLevels));
            embed.WithColor(Color.Green);

            embed.AddField("<:olut:329889326051753986> Experience", String.Format("{0}/{1} XP", profile.Experience, profile.ExperienceTarget), true);
            embed.AddField("<:geocache:357917503894061059> Current Level", String.Format("Level {0}", profile.ExperienceLevels), true);

            // Attempt to notify the user
            await MessageUtilities.SendPMSafely(Client.GetUser(profile.Identifier), null, String.Empty, false, embed.Build());
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
                EmbedBuilder embed2 = new EmbedBuilder();
                embed2.WithDescription(String.Format("Your <:eeg:359363156885110794> Health is now {0}/10 HP!", health));

                if (health < profile.Health)
                {
                    embed2.WithTitle("Oh perkele, you've lost health!");
                    embed2.WithColor(Color.Orange);
                    await MessageUtilities.SendPMSafely(Client.GetUser(profile.Identifier), null, String.Empty, false, embed2.Build());
                }

                if (health > profile.Health)
                {
                    embed2.WithTitle("Awesome, you've been healed!");
                    embed2.WithColor(Color.Green);
                    await MessageUtilities.SendPMSafely(Client.GetUser(profile.Identifier), null, String.Empty, false, embed2.Build());
                }

                profile.Health = health;
                await SaveProfile(profile);
                return;
            }

            profile.Health = 10;
            profile.OpenSewerTokens = 0;
            await SaveProfile(profile);

            // Build the embed
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Oh perkele, you've died!");
            embed.WithDescription("Your health has fallen too low and you have died. Game over, sorry. All your stats except for XP and levels have been reset.");
            embed.WithColor(Color.Orange);

            // Attempt to notify the user
            await MessageUtilities.SendPMSafely(Client.GetUser(profile.Identifier), null, String.Empty, false, embed.Build());
        }

        /// <summary>
        /// Checks whether a user can modify another user.
        /// </summary>
        /// <param name="actor">The user initiating the action.</param>
        /// <param name="subject">The user being modified.</param>
        public async Task<bool> CanUserModifyUser(IUser actor, IUser subject)
        {
            ulong ownerId = (await Client.GetApplicationInfoAsync()).Owner.Id;

            // Bypass checks if user is bot owner or they have guild administrator permissions
            if (actor.Id == ownerId || ((SocketGuildUser)actor).GuildPermissions.Administrator)
                return true;

            if (((SocketGuildUser)actor).Roles.Sum(r => r.Position) > ((SocketGuildUser)subject).Roles.Sum(r => r.Position))
                return true;

            return false;
        }

        public static async Task<bool> DoesUserHaveAnyRole(ICommandContext context, IServiceProvider services, params string[] roles)
        {
            PermitRolesAttribute permitRoles = new PermitRolesAttribute(roles);

            if (!(await permitRoles.CheckPermissionsAsync(context, null, services)).IsSuccess)
                return false;

            return true;
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
