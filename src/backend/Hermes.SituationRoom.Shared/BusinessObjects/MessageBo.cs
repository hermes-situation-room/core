#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record MessageBo(string Content,
    Guid SenderUid,
    Guid ChatUid,
    DateTime Timestamp)
{
    public Guid? Uid { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public bool IsViewOnce { get; init; }

    public bool IsConsumed { get; init; }
}