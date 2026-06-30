namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class TagRepository(IHermessituationRoomContext context) : ITagRepository
{
    public async Task AddAsync(TagBo tagBo)
    {
        ArgumentNullException.ThrowIfNull(tagBo);

        var newPost = new PostTag { PostUid = tagBo.PostUid, Tag = tagBo.Tag, };

        context.PostTags.Add(newPost);
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<string>> GetAllTagsFromPostAsync(Guid postUid)
    {
        var postTags = await context.PostTags
            .AsNoTracking()
            .Where(p => p.PostUid == postUid)
            .ToListAsync();

        return postTags.Select(pt => pt.Tag).ToList();
    }

    public async Task<Dictionary<Guid, IReadOnlyList<string>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids)
    {
        var postUidsList = postUids.ToList();
        if (!postUidsList.Any())
            return new Dictionary<Guid, IReadOnlyList<string>>();

        var postTags = await context.PostTags
            .AsNoTracking()
            .Where(pt => postUidsList.Contains(pt.PostUid))
            .ToListAsync();

        return postTags
            .GroupBy(pt => pt.PostUid)
            .ToDictionary(g => g.Key,
                g => (IReadOnlyList<string>)g.Select(pt => pt.Tag).ToList()
            );
    }

    public async Task<IReadOnlyList<string>> GetAllNamesAsync() =>
        await context.Tags
            .AsNoTracking()
            .Select(t => t.Name)
            .ToListAsync();

    public async Task<bool> ExistsByNameAsync(string name) =>
        await context.Tags
            .AsNoTracking()
            .AnyAsync(t => t.Name == name);

    public async Task AddTagAsync(string name)
    {
        if (await ExistsByNameAsync(name))
            return;

        context.Tags.Add(new Tag { Uid = Guid.NewGuid(), Name = name, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TagBo tagBo)
    {
        var toDelete = await context.PostTags
            .Where(p => p.PostUid == tagBo.PostUid && p.Tag == tagBo.Tag)
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
}
