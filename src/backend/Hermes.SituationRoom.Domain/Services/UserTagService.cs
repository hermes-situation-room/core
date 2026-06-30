namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public sealed class UserTagService(IUserTagRepository userTagRepository, IMapper mapper) : IUserTagService
{
    public async Task<UserTagDto> CreateAsync(Guid ownerUid, CreateUserTagDto createUserTagDto)
    {
        ArgumentNullException.ThrowIfNull(createUserTagDto);

        if (string.IsNullOrWhiteSpace(createUserTagDto.TagName))
        {
            throw new ValidationBusinessException(nameof(createUserTagDto.TagName), "Tag name is required.");
        }

        if (createUserTagDto.TaggedUserUid == Guid.Empty)
        {
            throw new ValidationBusinessException(nameof(createUserTagDto.TaggedUserUid), "Tagged user is required.");
        }

        // Owner is always the authenticated user; never trust the body's owner id.
        var userTagBo = new UserTagBo(
            Guid.NewGuid(),
            ownerUid,
            createUserTagDto.TaggedUserUid,
            createUserTagDto.TagName.Trim(),
            createUserTagDto.Notes,
            DateTime.UtcNow
        );

        var uid = await userTagRepository.AddAsync(userTagBo);
        var created = await userTagRepository.GetByUidAsync(uid);

        return mapper.Map<UserTagDto>(created);
    }

    public async Task<UserTagDto> UpdateAsync(Guid ownerUid, Guid uid, UpdateUserTagDto updateUserTagDto)
    {
        ArgumentNullException.ThrowIfNull(updateUserTagDto);

        if (string.IsNullOrWhiteSpace(updateUserTagDto.TagName))
        {
            throw new ValidationBusinessException(nameof(updateUserTagDto.TagName), "Tag name is required.");
        }

        var existing = await userTagRepository.GetByUidAsync(uid);

        // A user can only modify tags they created.
        if (existing == null || existing.OwnerUid != ownerUid)
        {
            throw new ValidationBusinessException(nameof(uid), "Tag not found.");
        }

        var updatedBo = existing with
        {
            TagName = updateUserTagDto.TagName.Trim(),
            Notes = updateUserTagDto.Notes
        };

        await userTagRepository.UpdateAsync(updatedBo);
        var updated = await userTagRepository.GetByUidAsync(uid);

        return mapper.Map<UserTagDto>(updated);
    }

    public async Task DeleteAsync(Guid ownerUid, Guid uid)
    {
        // Owner-scoped delete: only removes the tag if it belongs to this owner.
        await userTagRepository.DeleteAsync(uid, ownerUid);
    }

    public async Task<IReadOnlyList<UserTagDto>> GetMyTagsAsync(Guid ownerUid)
    {
        var tags = await userTagRepository.GetByOwnerAsync(ownerUid);
        return mapper.Map<IReadOnlyList<UserTagDto>>(tags);
    }

    public async Task<IReadOnlyList<UserTagDto>> GetMyTagsForUserAsync(Guid ownerUid, Guid taggedUserUid)
    {
        var tags = await userTagRepository.GetByOwnerAndTaggedAsync(ownerUid, taggedUserUid);
        return mapper.Map<IReadOnlyList<UserTagDto>>(tags);
    }

    public async Task<IReadOnlyList<UserTagDto>> SearchMyTagsAsync(Guid ownerUid, string tagName)
    {
        var tags = await userTagRepository.SearchByTagNameAsync(ownerUid, tagName ?? string.Empty);
        return mapper.Map<IReadOnlyList<UserTagDto>>(tags);
    }
}
