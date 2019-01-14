using System.ComponentModel.DataAnnotations;

namespace MorkoBotRavenEdition.Models
{
    internal class UserItem
    {
        [Key]
        public string Name { get; set; }
        public string VanityName { get; set; }
        public ulong UserId { get; set; }
        public ulong Guild { get; set; }
        public int Amount { get; set; }
        public int OriginalPrice { get; set; }
    }
}
