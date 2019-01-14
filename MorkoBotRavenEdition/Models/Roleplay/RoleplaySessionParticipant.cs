using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
