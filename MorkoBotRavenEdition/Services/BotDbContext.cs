using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MorkoBotRavenEdition.Models.Roleplay;

namespace MorkoBotRavenEdition.Services
{
    internal class BotDbContext : DbContext
    {
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserWarning> UserWarnings { get; set; }
        public DbSet<ExtendedGuildInfo> Guilds { get; set; }
        public DbSet<VanityRole> VanityRoles { get; set; }
        public DbSet<UserItem> UserItems { get; set; }

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
            //optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["MorkoDev"].ConnectionString);
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MorkoBot.Database;Trusted_Connection=True;MultipleActiveResultSets=true");
#else
            // Use production SQL server database
            //optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["MorkoBot"].ConnectionString);
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MorkoBot.Database;Trusted_Connection=True;MultipleActiveResultSets=true");
#endif
        }
    }
}
