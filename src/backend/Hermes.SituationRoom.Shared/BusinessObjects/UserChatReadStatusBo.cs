#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserChatReadStatus(Guid UserId,
    Guid ChatId,
    DateTime ReadTime
)
{
    private Guid? Uid;
}
