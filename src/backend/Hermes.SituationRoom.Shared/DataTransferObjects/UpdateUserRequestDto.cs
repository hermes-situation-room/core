#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UpdateUserRequestDto(Guid Uid,
    string? FirstName,
    string? LastName,
    string? EmailAddress
);
