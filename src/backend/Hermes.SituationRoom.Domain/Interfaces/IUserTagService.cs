namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IUserTagService
{
    Task<UserTagDto> CreateAsync(Guid ownerUid, CreateUserTagDto createUserTagDto);
    Task<UserTagDto> UpdateAsync(Guid ownerUid, Guid uid, UpdateUserTagDto updateUserTagDto);
    Task DeleteAsync(Guid ownerUid, Guid uid);
    Task<IReadOnlyList<UserTagDto>> GetMyTagsAsync(Guid ownerUid);
    Task<IReadOnlyList<UserTagDto>> GetMyTagsForUserAsync(Guid ownerUid, Guid taggedUserUid);
    Task<IReadOnlyList<UserTagDto>> SearchMyTagsAsync(Guid ownerUid, string tagName);
}
