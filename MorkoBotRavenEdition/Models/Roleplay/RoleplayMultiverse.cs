using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MorkoBotRavenEdition.Models.Roleplay
{
    internal class RoleplayMultiverse
    {
        [Key]
        public int MultiverseId { get; set; }
        public string MultiverseName { get; set; }

        // Guild info
        public ulong GuildId { get; set; }
    }
}
