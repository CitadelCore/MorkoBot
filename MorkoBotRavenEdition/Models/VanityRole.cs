using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MorkoBotRavenEdition.Models
{
    class VanityRole
    {
        [Key]
        public ulong Id { get; set; }
        public ulong Guild { get; set; }
        public string Name { get; set; }

        [NotMapped]
        public RoleRestrictionLevel RestrictionLevel
        {
            get { return _RestrictionLevel == null ? RoleRestrictionLevel.ManualOnly : JsonConvert.DeserializeObject<RoleRestrictionLevel>(_RestrictionLevel); }
            set { _RestrictionLevel = JsonConvert.SerializeObject(value); }
        }
        public string _RestrictionLevel { get; set; }

        public enum RoleRestrictionLevel
        {
            Unrestricted,
            RequestOnly,
            ManualOnly
        }
    }
}
