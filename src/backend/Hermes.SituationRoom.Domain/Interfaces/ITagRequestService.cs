namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface ITagRequestService
{
    Task<TagRequestDto> CreateTagRequestAsync(Guid requestedByUid, string name);

    Task<IReadOnlyList<TagRequestDto>> GetMyTagRequestsAsync(Guid requestedByUid);

    Task<IReadOnlyList<TagRequestDto>> GetPendingTagRequestsAsync();

    Task<TagRequestDto> ApproveTagRequestAsync(Guid uid, Guid reviewerUid);

    Task<TagRequestDto> RejectTagRequestAsync(Guid uid, Guid reviewerUid);
}
