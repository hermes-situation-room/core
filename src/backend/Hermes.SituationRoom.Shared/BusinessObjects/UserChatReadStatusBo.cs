#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserChatReadStatusBo(Guid UserId,
    Guid ChatId,
    DateTime ReadTime
)
{
    public Guid? Uid;
}
