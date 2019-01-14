using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MorkoBotRavenEdition.Models.Roleplay
{
    /// <summary>
    /// Holds information about a single roleplay "session",
    /// one or more units of time in which characters are actively playing.
    /// </summary>
    internal class RoleplaySession
    {
        [Key]
        public int SessionId { get; set; }

        // Multiverse info
        public int MultiverseId { get; set; }
        public RoleplayMultiverse Multiverse { get; set; }

        // Specifics
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Active { get; set; }
        public bool Paused { get; set; }
    }
}
