using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models.Roleplay;
using System.Configuration;

namespace MorkoBotRavenEdition.Services
{
    /// <summary>
    /// Roleplay DB context, for connecting to RPDB
    /// </summary>
    internal class RPDbContext : DbContext
    {
        public DbSet<RoleplayCharacter> Characters { get; set; }
        public DbSet<RoleplayMultiverse> Multiverses { get; set; }
        public DbSet<RoleplayReality> Realities { get; set; }
        public DbSet<RoleplaySession> Sessions { get; set; }
        public DbSet<RoleplaySessionParticipant> SessionParticipants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            // Use temporary development database
            var str = "server=localhost;port=3306;database=rpdb;user=morkobot;password=hqE6k87GRZMQAqtRXtMvY4";
            optionsBuilder.UseMySql(str);

#else
            // Use production SQL server database
            optionsBuilder.UseMySql(ConfigurationManager.ConnectionStrings["RPDB"].ConnectionString);
#endif
        }
    }
}
