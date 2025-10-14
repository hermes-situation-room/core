#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserBo(Guid Uid,
    string? Password,
    byte[]? PasswordHash,
    byte[]? PasswordSalt,
    string? FirstName,
    string? LastName,
    string? EmailAddress
);
