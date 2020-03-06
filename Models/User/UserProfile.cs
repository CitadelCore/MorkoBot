using System;
using System.ComponentModel.DataAnnotations;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.User
{
    internal class UserProfile
    {
        [Key]
        public long Identifier { get; set; }
        public long GuildIdentifier { get; set; }
        public int OpenSewerTokens { get; set; }
        public int Experience { get; set; }
        public int ExperienceTarget { get; set; } = 10;
        public int ExperienceLevels { get; set; }
        public int IncrementCount { get; set; }
        public DateTime LastIncremented { get; set; }

        // Basis for new Health system
        // If your health falls below 1, you die, and your stats are reset
        public int Health { get; set; } = 10;

        //public ICollection<ShopItem> Inventory = new List<ShopItem>();
        //public ICollection<StatusEffect> StatusEffects = new List<StatusEffect>();
    }
}
