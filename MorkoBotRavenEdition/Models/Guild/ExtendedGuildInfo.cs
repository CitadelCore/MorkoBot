using System.ComponentModel.DataAnnotations;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Guild
{
    /// <summary>
    /// Holds extended information for a Discord guild.
    /// </summary>
    internal class ExtendedGuildInfo
    {
        /// <summary>
        /// Guild identifier. This should correspond to the
        /// unique Guild ID assigned by Discord.
        /// </summary>
        [Key]
        public long Identifier { get; set; }

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
