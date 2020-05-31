// this is a very dirty hack, but it's all we can do for now
extern alias SystemAsync;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using MorkoBotRavenEdition.Utilities;

namespace MorkoBotRavenEdition.Models.Discord
{
    public class FakeUserMessage : IUserMessage
    {
        public FakeUserMessage(IMessageChannel channel, IUser user, string content) {
            Channel = channel;
            Author = user;
            Content = content;    
        }

        public MessageType Type { get; set; } = MessageType.Default;

        public MessageSource Source { get; set; } = MessageSource.User;

        public bool IsTTS { get; set; } = false;

        public bool IsPinned { get; set; } = false;

        public bool IsSuppressed { get; set; } = false;

        public string Content { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTime.Now;

        public DateTimeOffset? EditedTimestamp { get; set; }

        public IMessageChannel Channel { get; set; }

        public IUser Author { get; set; }

        public IReadOnlyCollection<IAttachment> Attachments { get; set; } = new List<Attachment>();

        public IReadOnlyCollection<IEmbed> Embeds { get; set; } = new List<IEmbed>();

        public IReadOnlyCollection<ITag> Tags { get; set; } = new List<ITag>();

        public IReadOnlyCollection<ulong> MentionedChannelIds { get; set; } = new List<ulong>();

        public IReadOnlyCollection<ulong> MentionedRoleIds { get; set; } = new List<ulong>();

        public IReadOnlyCollection<ulong> MentionedUserIds { get; set; } = new List<ulong>();

        public MessageActivity Activity { get; set; }

        public MessageApplication Application { get; set; }

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions { get; set; } = new Dictionary<IEmote, ReactionMetadata>();

        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;

        public ulong Id { get; set; } = (ulong) MessageUtilities.RandomMessageId();

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
            => Task.CompletedTask;

        public Task DeleteAsync(RequestOptions options = null)
            => Task.CompletedTask;

        public SystemAsync.System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
            => Task.CompletedTask;

        public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions options = null)
            => Task.CompletedTask;

        public Task PinAsync(RequestOptions options = null)
            => Task.CompletedTask;

        public Task RemoveAllReactionsAsync(RequestOptions options = null)
            => Task.CompletedTask;

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
            => Task.CompletedTask;

        public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null)
            => Task.CompletedTask;

        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
            => Content;

        public Task UnpinAsync(RequestOptions options = null)
            => Task.CompletedTask;
    }
}