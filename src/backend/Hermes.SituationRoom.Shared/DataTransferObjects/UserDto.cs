#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UserDto(Guid Uid,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? ProfileIcon,
    string? ProfileIconColor
);
