#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserBo(Guid Uid,
    string Password,
    string? FirstName,
    string? LastName,
    string? EmailAddress
);
