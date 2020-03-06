using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MorkoBotRavenEdition.Models.Infra
{
    public class InfraCompetitionEntry
    {
        /// <summary>
        /// The identifier of the message that contains the entry.
        /// </summary>
        [Key]
        public long Identifier { get; set; }

        /// <summary>
        /// The User ID of the entrant.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// The time at which the entry was submitted.
        /// </summary>
        public DateTime Entered { get; set; }

        /// <summary>
        /// Whether the entry has been deleted (and removed from scoring).
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// The URL of the entry image.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// The number of votes the entry has recieved.
        /// </summary>
        public int Votes { get; set; } = 0;
    }
}
