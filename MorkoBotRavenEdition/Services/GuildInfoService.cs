using Discord.WebSocket;
using MorkoBotRavenEdition.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MorkoBotRavenEdition.Models.VanityRole;

namespace MorkoBotRavenEdition.Services
{
    /// <summary>
    /// Responsible for providing and storing 
    /// extended information about a guild.
    /// </summary>
    internal class GuildInfoService
    {
        private readonly DiscordSocketClient _client;
        private readonly BotDbContext _context;
        public GuildInfoService(DiscordSocketClient dsc, BotDbContext dbContext)
        {
            _client = dsc;
            _context = dbContext;
        }

        /// <summary>
        /// Retrieves extended guild information.
        /// If the guild does not exist in the database, it will be created.
        /// </summary>
        public async Task<ExtendedGuildInfo> GetGuildInfo(ulong id)
        {
            var guildInfo = _context.Guilds.FirstOrDefault(g => g.Identifier == id);

            if (guildInfo != null) return guildInfo;

            guildInfo = new ExtendedGuildInfo() { Identifier = id };
            _context.Add(guildInfo);
            await _context.SaveChangesAsync();

            return guildInfo;
        }

        /// <summary>
        /// Saves changes to a guild information object.
        /// </summary>
        public async Task SaveGuildInfo(ExtendedGuildInfo guild)
        {
            _context.Update(guild);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Clears guild information from the database.
        /// </summary>
        public async Task DeleteGuild(ulong id)
        {
            var guildInfo = _context.Guilds.FirstOrDefault(g => g.Identifier == id);

            if (guildInfo != null)
            {
                _context.Remove(guildInfo);
                await _context.SaveChangesAsync();
            }

            IEnumerable<VanityRole> roles = _context.VanityRoles.Where(r => r.Guild == id);

            if (roles.Any())
                _context.RemoveRange(roles);
        }

        public VanityRole GetRole(ulong id, ulong guild)
        {
            return _context.VanityRoles.FirstOrDefault(r => r.Id == id && r.Guild == guild);
        }

        public IEnumerable<VanityRole> GetAllRoles(ulong guild)
        {
            return _context.VanityRoles.Where(r => r.Guild == guild);
        }

        public async Task AddVanityRole(ulong id, ulong guild, string name, RoleRestrictionLevel level)
        {
            _context.VanityRoles.Add(new VanityRole() { Id = id, Guild = guild, Name = name, RestrictionLevel = level });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveVanityRole(ulong id, ulong guild)
        {
            var role = _context.VanityRoles.FirstOrDefault(r => r.Id == id && r.Guild == guild);
            if (role != null)
            {
                _context.Remove(role);
                await _context.SaveChangesAsync();
            } 
        }

        public async Task UpdateVanityRole(ulong id, ulong guild, RoleRestrictionLevel level)
        {
            var role = _context.VanityRoles.FirstOrDefault(r => r.Id == id && r.Guild == guild);
            if (role != null)
            {
                role.RestrictionLevel = level;
                _context.Update(role);
                await _context.SaveChangesAsync();
            }
        }
    }
}
