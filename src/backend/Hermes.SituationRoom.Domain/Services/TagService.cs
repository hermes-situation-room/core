namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public class TagService(ITagRepository repository) : ITagService
{
    public async Task CreatePostTagAsync(CreateTagDto createTagDto)
    {
        var tag = await ValidateTagNameAsync(createTagDto.Tag);
        await repository.AddAsync(new(createTagDto.PostUid, tag));
    }

    public async Task<IReadOnlyList<string>> GetAllTagsFromPostAsync(Guid postUid) =>
        await repository.GetAllTagsFromPostAsync(postUid);

    public async Task<Dictionary<Guid, IReadOnlyList<string>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids) =>
        await repository.GetAllTagsFromPostsAsync(postUids);

    public async Task<IReadOnlyList<string>> GetAllTags() => await repository.GetAllNamesAsync();

    public async Task DeletePostTagAsync(TagBo tagBo) => await repository.DeleteAsync(tagBo);

    public async Task<string> ValidateTagNameAsync(string tag)
    {
        if (!await repository.ExistsByNameAsync(tag))
            throw new ValidationBusinessException("Tag", $"Tag '{tag}' is not a valid post tag.");

        return tag;
    }
}
