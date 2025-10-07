#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record JournalistBo(Guid Uid,
    string Password,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string Employer
)
    : UserBo(Uid,
        Password,
        FirstName,
        LastName,
        EmailAddress
    );
