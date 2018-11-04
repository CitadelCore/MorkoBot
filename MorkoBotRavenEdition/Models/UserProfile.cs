using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Models
{
    class UserProfile
    {
        [Key]
        public ulong Identifier { get; set; }
        public ulong GuildIdentifier { get; set; }
        public int OpenSewerTokens { get; set; } = 0;
        public int Experience { get; set; } = 0;
        public int ExperienceTarget { get; set; } = 10;
        public int ExperienceLevels { get; set; } = 0;
        public int IncrementCount { get; set; } = 0;
        public DateTime LastIncremented { get; set; }

        // Basis for new Health system
        // If your health falls below 1, you die, and your stats are reset
        public int Health { get; set; } = 10;

        //public ICollection<ShopItem> Inventory = new List<ShopItem>();
        //public ICollection<StatusEffect> StatusEffects = new List<StatusEffect>();
    }
}
