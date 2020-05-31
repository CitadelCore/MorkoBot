using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Roleplay
{
    internal class RoleplayCharacter
    {
        public enum GenderType
        {
            Default,
            Male,
            Female
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Holds the Discord ID of the player who created
        /// the character. This player is the only person who can
        /// transfer ownership of the character, or assign it to someone else.
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Holds the Discord ID of the player who is currently
        /// playing the character. This is usually the same as the owner,
        /// however it may be assigned to someone else.
        /// </summary>
        public int PlayerId { get; set; }

        // Character personalisation
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int Age { get; set; }
        public GenderType Gender { get; set; }
        public string ImageUri { get; set; }

        // Description
        public string Appearance { get; set; }
        public string Personality { get; set; }
        public string Backstory { get; set; }
    }
}
