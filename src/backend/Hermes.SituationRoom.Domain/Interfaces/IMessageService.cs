#nullable enable
namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IMessageService
{
    Task<Guid> AddAsync(CreateMessageRequestDto createMessageDto);

    /// <summary>
    /// Fetches a single message as seen by <paramref name="requestingUserId"/>, applying the same
    /// expiry and view-once rules as the list path. This is the consume/reveal path for recipients:
    /// a view-once message is returned to a recipient exactly once and blocked thereafter. Throws
    /// <see cref="KeyNotFoundException"/> when no content may be returned (missing/expired/consumed).
    /// </summary>
    Task<MessageDto> GetMessageAsync(Guid messageId, Guid requestingUserId);

    Task<IReadOnlyList<MessageDto>> GetMessagesByChatAsync(Guid chatId, Guid requestingUserId);

    Task UpdateAsync(Guid messageId, string newContent);

    Task DeleteAsync(Guid messageId);

    /// <summary>
    /// Marks a view-once message as viewed by the given user and broadcasts its removal so it
    /// disappears live for that recipient. Returns false if the message is not a view-once message.
    /// </summary>
    Task<bool> MarkViewedAsync(Guid messageId, Guid userId);

    /// <summary>
    /// Purges all expired (disappeared) messages and broadcasts their removal. Returns purged count.
    /// </summary>
    Task<int> PurgeExpiredAsync();
}
