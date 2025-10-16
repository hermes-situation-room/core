#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UpdateActivistRequestDto(Guid Uid,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string UserName,
    bool IsFirstNameVisible,
    bool IsLastNameVisible,
    bool IsEmailVisible
);
