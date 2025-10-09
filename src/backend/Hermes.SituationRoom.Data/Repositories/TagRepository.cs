namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;
using Shared.EnumClasses;

public sealed class TagRepository(IHermessituationRoomContext context) : ITagRepository
{
    public async Task AddAsync(TagBo tagBo)
    {
        ArgumentNullException.ThrowIfNull(tagBo);

        var newPost = new PostTag { PostUid = tagBo.PostUid, Tag = tagBo.Tag.ToString(), };

        context.PostTags.Add(newPost);
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Tag>> GetAllTagsFromPostAsync(Guid postUid)
    {
        var postTags = await context.PostTags
            .AsNoTracking()
            .Where(p => p.PostUid == postUid)
            .ToListAsync();

        return postTags.Select(MapToTag).ToList();
    }

    public async Task<Dictionary<Guid, IReadOnlyList<Tag>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids)
    {
        var postUidsList = postUids.ToList();
        if (!postUidsList.Any())
            return new Dictionary<Guid, IReadOnlyList<Tag>>();

        var postTags = await context.PostTags
            .AsNoTracking()
            .Where(pt => postUidsList.Contains(pt.PostUid))
            .ToListAsync();

        return postTags
            .GroupBy(pt => pt.PostUid)
            .ToDictionary(g => g.Key,
                g => (IReadOnlyList<Tag>)g.Select(MapToTag).ToList()
            );
    }

    public IReadOnlyList<Tag> GetAllTags() => Enum.GetValues(typeof(Tag)).Cast<Tag>().ToList();

    public async Task DeleteAsync(TagBo tagBo)
    {
        var toDelete = await context.PostTags
            .Where(p => p.PostUid == tagBo.PostUid && p.Tag == tagBo.Tag.ToString())
            .SingleOrDefaultAsync();

        if (toDelete != null)
        {
            context.PostTags.Remove(toDelete);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<PostTag>> GetAllPostTags()
    {
        return await context.PostTags
            .AsNoTracking()
            .ToListAsync();
    }

    private static Tag MapToTag(PostTag postTag) => Enum.Parse<Tag>(postTag.Tag);
}
