using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MorkoBotRavenEdition.Models
{
    /// <summary>
    /// Holds extended information for a Discord guild.
    /// </summary>
    class ExtendedGuildInfo
    {
        /// <summary>
        /// Guild identifier. This should correspond to the
        /// unique Guild ID assigned by Discord.
        /// </summary>
        [Key]
        public ulong Identifier { get; set; }

        /// <summary>
        /// Number of increments currently.
        /// </summary>
        public int IncrementCount { get; set; } = 0;

        /// <summary>
        /// Increment target count.
        /// </summary>
        public int IncrementTarget { get; set; } = 10;

        /// <summary>
        /// Default channel to send bot notifications to.
        /// </summary>
        public string DefaultChannel { get; set; }
    }
}
