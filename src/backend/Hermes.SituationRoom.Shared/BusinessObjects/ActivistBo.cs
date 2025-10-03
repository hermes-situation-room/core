#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record ActivistBo(Guid Uid,
    string Password,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string UserName,
    bool IsFirstNameVisible,
    bool IsLastNameVisible,
    bool IsEmailVisible
)
    : UserBo(Uid,
        Password,
        FirstName,
        LastName,
        EmailAddress
    );
