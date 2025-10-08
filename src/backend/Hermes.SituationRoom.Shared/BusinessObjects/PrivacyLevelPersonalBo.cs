#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record PrivacyLevelPersonalBo(Guid Uid,
    bool IsFirstNameVisible,
    bool IsLastNameVisible,
    bool IsEmailVisible,
    Guid OwnerUid,
    Guid ConsumerUid
);
