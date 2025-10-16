#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record LoginActivistRequestDto(string Password,
    string UserName
);
