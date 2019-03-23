using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Roleplay
{
    internal class RoleplaySessionParticipant
    {
        [Key]
        public int ParticipantId { get; set; }
        public int CharacterId { get; set; }
        public int SessionId { get; set; }
    }
}
