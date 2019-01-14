using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MorkoBotRavenEdition.Models
{
    internal class VanityRole
    {
        [Key]
        public ulong Id { get; set; }
        public ulong Guild { get; set; }
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
