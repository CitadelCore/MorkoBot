﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Roleplay
{
    internal class RoleplayReality
    {
        // Reality info
        [Key]
        public int Id { get; set; }
        public int MultiverseId { get; set; }
        public RoleplayMultiverse Multiverse { get; set; }

        // Specifics
        public string RealityName { get; set; }
    }
}
