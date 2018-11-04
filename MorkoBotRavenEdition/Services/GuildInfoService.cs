using Discord.WebSocket;
using MorkoBotRavenEdition.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using static MorkoBotRavenEdition.Models.ExtendedGuildInfo;
using static MorkoBotRavenEdition.Models.VanityRole;

namespace MorkoBotRavenEdition.Services
{
    /// <summary>
    /// Responsible for providing and storing 
    /// extended information about a guild.
    /// </summary>
    class GuildInfoService
    {
        private DiscordSocketClient Client;
        private BotDbContext Context;
        public GuildInfoService(DiscordSocketClient dsc, BotDbContext dbContext)
        {
            Client = dsc;
            Context = dbContext;
        }

        /// <summary>
        /// Retrieves extended guild information.
        /// If the guild does not exist in the database, it will be created.
        /// </summary>
        public async Task<ExtendedGuildInfo> GetGuildInfo(ulong id)
        {
            ExtendedGuildInfo guildInfo = Context.Guilds.Where(g => g.Identifier == id).FirstOrDefault();

            if (guildInfo == null)
            {
                guildInfo = new ExtendedGuildInfo() { Identifier = id };
                Context.Add(guildInfo);
                await Context.SaveChangesAsync();
            }

            return guildInfo;
        }

        /// <summary>
        /// Saves changes to a guild information object.
        /// </summary>
        public async Task SaveGuildInfo(ExtendedGuildInfo guild)
        {
            Context.Update(guild);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Clears guild information from the database.
        /// </summary>
        public async Task DeleteGuild(ulong id)
        {
            ExtendedGuildInfo guildInfo = Context.Guilds.Where(g => g.Identifier == id).FirstOrDefault();

            if (guildInfo != null)
            {
                Context.Remove(guildInfo);
                await Context.SaveChangesAsync();
            }

            IEnumerable<VanityRole> roles = Context.VanityRoles.Where(r => r.Guild == id);

            if (roles != null && roles.Count() > 0)
                Context.RemoveRange(roles);
        }

        public VanityRole GetRole(ulong id, ulong guild)
        {
            return Context.VanityRoles.Where(r => r.Id == id && r.Guild == guild).FirstOrDefault();
        }

        public IEnumerable<VanityRole> GetAllRoles(ulong guild)
        {
            return Context.VanityRoles.Where(r => r.Guild == guild);
        }

        public async Task AddVanityRole(ulong id, ulong guild, string name, RoleRestrictionLevel level)
        {
            Context.VanityRoles.Add(new VanityRole() { Id = id, Guild = guild, Name = name, RestrictionLevel = level });
            await Context.SaveChangesAsync();
        }

        public async Task RemoveVanityRole(ulong id, ulong guild)
        {
            VanityRole role = Context.VanityRoles.Where(r => r.Id == id).FirstOrDefault();
            if (role != null)
            {
                Context.Remove(role);
                await Context.SaveChangesAsync();
            } 
        }

        public async Task UpdateVanityRole(ulong id, ulong guild, RoleRestrictionLevel level)
        {
            VanityRole role = Context.VanityRoles.Where(r => r.Id == id).FirstOrDefault();
            if (role != null)
            {
                role.RestrictionLevel = level;
                Context.Update(role);
                await Context.SaveChangesAsync();
            }
        }
    }
}
