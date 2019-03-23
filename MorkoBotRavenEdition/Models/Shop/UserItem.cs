using System.ComponentModel.DataAnnotations;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Shop
{
    internal class UserItem
    {
        [Key]
        public string Name { get; set; }
        public string VanityName { get; set; }
        public long UserId { get; set; }
        public long Guild { get; set; }
        public int Amount { get; set; }
        public int OriginalPrice { get; set; }
    }
}
