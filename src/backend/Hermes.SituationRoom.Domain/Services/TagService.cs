namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Enums;

public class TagService(ITagRepository repository) : ITagService
{
    public async Task CreatePostTagAsync(CreateTagDto createTagDto)
    {
        ArgumentNullException.ThrowIfNull(createTagDto, nameof(createTagDto));

        if (createTagDto.PostUid == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty.", nameof(createTagDto.PostUid));

        if (string.IsNullOrWhiteSpace(createTagDto.Tag))
            throw new ArgumentException("Tag cannot be empty.", nameof(createTagDto.Tag));

        var tag = MapToTag(createTagDto.Tag);
        await repository.AddAsync(new(createTagDto.PostUid, tag));
    }

    public async Task<IReadOnlyList<Tag>> GetAllTagsFromPostAsync(Guid postUid)
    {
        if (postUid == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty.", nameof(postUid));

        return await repository.GetAllTagsFromPostAsync(postUid);
    }

    public async Task<Dictionary<Guid, IReadOnlyList<Tag>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids)
    {
        ArgumentNullException.ThrowIfNull(postUids, nameof(postUids));

        return await repository.GetAllTagsFromPostsAsync(postUids);
    }

    public IReadOnlyList<string> GetAllTags() => repository.GetAllTags().Select(t => t.ToString()).ToList();

    public async Task DeletePostTagAsync(TagBo tagBo)
    {
        ArgumentNullException.ThrowIfNull(tagBo, nameof(tagBo));

        if (tagBo.PostUid == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty.", nameof(tagBo.PostUid));

        await repository.DeleteAsync(tagBo);
    }
    
    public static Tag MapToTag(string tag)
    {
        try
        {
            return Enum.Parse<Tag>(tag);
        }
        catch (Exception e)
        {
            throw new ArgumentException($"Tag '{tag}' is not valid.", e);
        }
    }
}
