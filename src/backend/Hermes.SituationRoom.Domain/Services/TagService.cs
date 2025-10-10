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
        var tag = MapToTag(createTagDto.Tag);
        await repository.AddAsync(new(createTagDto.PostUid, tag));
    }

    public async Task<IReadOnlyList<Tag>> GetAllTagsFromPostAsync(Guid postUid) =>
        await repository.GetAllTagsFromPostAsync(postUid);

    public async Task<Dictionary<Guid, IReadOnlyList<Tag>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids) =>
        await repository.GetAllTagsFromPostsAsync(postUids);

    public IReadOnlyList<string> GetAllTags() => repository.GetAllTags().Select(t => t.ToString()).ToList();

    public async Task DeletePostTagAsync(TagBo tagBo) => await repository.DeleteAsync(tagBo);
    
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
