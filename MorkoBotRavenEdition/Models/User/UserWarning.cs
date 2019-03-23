using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.User
{
    internal class UserWarning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public long GuildIdentifier { get; set; }
        public long UserId { get; set; }
        public long StaffId { get; set; }
        public string Reason { get; set; }
        public DateTime TimeAdded { get; set; }
        public int DaysExpiry { get; set; }
    }
}
