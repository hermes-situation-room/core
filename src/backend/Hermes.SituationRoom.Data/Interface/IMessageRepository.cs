namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IMessageRepository
{
    Task<Guid> AddAsync(MessageBo newMessageBo);

    Task<MessageBo> GetMessageAsync(Guid messageId);

    /// <summary>
    /// Single-message fetch with the same view-once / expiry rules as the list path, as seen by
    /// <paramref name="userId"/>. Returns null (no content) when the message is missing, has
    /// expired, or is a view-once message a recipient has already consumed. For a recipient's FIRST
    /// fetch of a view-once message this atomically records the view (consume) and returns content
    /// exactly once. The sender always sees their own view-once content without consuming it.
    /// </summary>
    Task<MessageBo?> GetMessageForUserAsync(Guid messageId, Guid userId);

    Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId);

    /// <summary>
    /// Returns messages for a chat as seen by <paramref name="userId"/>: expired messages are
    /// purged/excluded and view-once messages already consumed by this user are blanked.
    /// </summary>
    Task<IReadOnlyList<MessageBo>> GetMessagesByChatForUserAsync(Guid chatId, Guid userId);

    Task<MessageBo> UpdateAsync(Guid messageId, string newContent);

    Task DeleteAsync(Guid messageId);

    /// <summary>
    /// Records that <paramref name="userId"/> has viewed a view-once message. Returns false if the
    /// message does not exist or is not view-once.
    /// </summary>
    Task<bool> MarkViewedAsync(Guid messageId, Guid userId);

    /// <summary>
    /// Physically removes every message whose disappear timer has passed. Returns the
    /// (chat id, message id) pairs that were purged so removals can be broadcast per chat.
    /// </summary>
    Task<IReadOnlyList<(Guid ChatUid, Guid MessageUid)>> PurgeExpiredAsync();
}