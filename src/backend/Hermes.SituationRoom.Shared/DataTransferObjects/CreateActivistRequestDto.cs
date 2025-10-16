#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateActivistRequestDto(string Password,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? ProfileIcon,
    string? ProfileIconColor,
    string UserName,
    bool IsFirstNameVisible,
    bool IsLastNameVisible,
    bool IsEmailVisible
);
