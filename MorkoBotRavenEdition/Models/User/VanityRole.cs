using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.User
{
    internal class VanityRole
    {
        [Key]
        public long Id { get; set; }
        public long Guild { get; set; }
        public string Name { get; set; }

        [NotMapped]
        public RoleRestrictionLevel RestrictionLevel
        {
            get => RestrictionLevelInternal == null ? RoleRestrictionLevel.ManualOnly : JsonConvert.DeserializeObject<RoleRestrictionLevel>(RestrictionLevelInternal);
            set => RestrictionLevelInternal = JsonConvert.SerializeObject(value);
        }

        private string RestrictionLevelInternal { get; set; }

        public enum RoleRestrictionLevel
        {
            Unrestricted,
            RequestOnly,
            ManualOnly
        }
    }
}
