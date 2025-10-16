#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateUserRequestDto(string Password,
    string? FirstName,
    string? LastName,
    string? EmailAddress
);
