using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Services
{
    class BotDbContext : DbContext
    {
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserWarning> UserWarnings { get; set; }
        public DbSet<ExtendedGuildInfo> Guilds { get; set; }
        public DbSet<VanityRole> VanityRoles { get; set; }
        public DbSet<UserItem> UserItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            // Use temporary development database
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["MorkoDev"].ConnectionString);
            //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MorkoBotRavenEdition.Database;Trusted_Connection=True;MultipleActiveResultSets=true");
#else
            // Use production SQL server database
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["MorkoBot"].ConnectionString);
#endif
        }
    }
}
