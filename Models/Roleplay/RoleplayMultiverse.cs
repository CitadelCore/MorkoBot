using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Roleplay
{
    internal class RoleplayMultiverse
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        // Guild info
        public long Guild { get; set; }
    }
}
