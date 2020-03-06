using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Roleplay
{
    internal class RoleplayMultiverse
    {
        [Key]
        public int MultiverseId { get; set; }
        public string MultiverseName { get; set; }

        // Guild info
        public long GuildId { get; set; }
    }
}
