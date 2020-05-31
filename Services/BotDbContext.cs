using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models.Guild;
using MorkoBotRavenEdition.Models.Infra;
using MorkoBotRavenEdition.Models.User;
using System.Configuration;

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
