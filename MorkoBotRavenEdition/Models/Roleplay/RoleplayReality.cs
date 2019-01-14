using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MorkoBotRavenEdition.Models.Roleplay
{
    internal class RoleplayReality
    {
        // Reality info
        [Key]
        public int RealityId { get; set; }
        public int MultiverseId { get; set; }
        public RoleplayMultiverse Multiverse { get; set; }

        // Specifics
        public string RealityName { get; set; }
    }
}
