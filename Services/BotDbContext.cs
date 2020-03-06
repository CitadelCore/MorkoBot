using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MorkoBotRavenEdition.Models.Guild;
using MorkoBotRavenEdition.Models.Infra;
using MorkoBotRavenEdition.Models.Roleplay;
using MorkoBotRavenEdition.Models.User;

namespace MorkoBotRavenEdition.Services
{
    internal class BotDbContext : DbContext
    {
        // User
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserWarning> UserWarnings { get; set; }

        // INFRA
        public DbSet<InfraCompetitionEntry> InfraCompetitionEntries { get; set; }

        // Misc
        public DbSet<ExtendedGuildInfo> Guilds { get; set; }
        public DbSet<VanityRole> VanityRoles { get; set; }
        public DbSet<LoggedMessage> LoggedMessages { get; set; }

#if ROLEPLAY_ENABLED
        // Roleplay
        public DbSet<RoleplayCharacter> RoleplayCharacters { get; set; }
        public DbSet<RoleplayMultiverse> RoleplayMultiverses { get; set; }
        public DbSet<RoleplayReality> RoleplayRealities { get; set; }
        public DbSet<RoleplaySession> RoleplaySessions { get; set; }
        public DbSet<RoleplaySessionParticipant> RoleplaySessionParticipants { get; set; }
#endif

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            // Use temporary development database
            var str = "server=localhost;port=3306;database=morkobot;user=morkobot;password=hqE6k87GRZMQAqtRXtMvY4";
            optionsBuilder.UseMySql(str);

#else
            // Use production SQL server database
            optionsBuilder.UseMySql(ConfigurationManager.ConnectionStrings["MorkoBot"].ConnectionString);
#endif
        }
    }
}
