using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;
using Discord;
using Discord.WebSocket;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Guild
{
    /// <summary>
    /// Represents a single message sent by a user.
    /// </summary>
    public class LoggedMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long Guild { get; set; }
        public long Author { get; set; }
        public long Channel { get; set; }
        public long Message { get; set; }

        /// <summary>
        /// Timestamp of when this message was sent (or edited).
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Content of the message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Id of the original message, if this one is an edit.
        /// </summary>
        public long OriginalId { get; set; }

        /// <summary>
        /// Whether this message has been deleted
        /// </summary>
        public bool Deleted { get; set; } = false;

        public static LoggedMessage FromDiscordMessage(IUserMessage message)
        {
            long guild = 0;
            if (message.Channel is IGuildChannel guildChannel)
                guild = (long)guildChannel.Guild.Id;

            return new LoggedMessage
            {
                Author = (long) message.Author.Id,
                Channel = (long) message.Channel.Id,
                Content = message.Resolve(TagHandling.FullName),
                Guild = guild,
                Message = (long) message.Id,
                TimeStamp = message.Timestamp,
            };
        }
    }
}
