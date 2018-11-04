using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MorkoBotRavenEdition.Models
{
    class UserItem
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
