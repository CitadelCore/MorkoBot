using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Models
{
    class UserWarning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public ulong GuildIdentifier { get; set; }
        public ulong UserId { get; set; }
        public ulong StaffId { get; set; }
        public string Reason { get; set; }
        public DateTime TimeAdded { get; set; }
        public int DaysExpiry { get; set; }
    }
}
