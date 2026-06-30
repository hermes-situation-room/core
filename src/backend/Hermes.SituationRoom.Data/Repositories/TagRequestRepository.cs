namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class TagRequestRepository(IHermessituationRoomContext context) : ITagRequestRepository
{
    public async Task<Guid> AddAsync(TagRequestBo tagRequestBo)
    {
        ArgumentNullException.ThrowIfNull(tagRequestBo);

        var newTagRequest = new TagRequest
        {
            Uid = Guid.NewGuid(),
            RequestedByUid = tagRequestBo.RequestedByUid,
            Name = tagRequestBo.Name,
            Status = tagRequestBo.Status,
            ReviewedByUid = null,
            ReviewedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        context.TagRequests.Add(newTagRequest);
        await context.SaveChangesAsync();

        return newTagRequest.Uid;
    }

    public async Task<TagRequestBo?> GetByIdAsync(Guid uid)
    {
        var tagRequest = await context.TagRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(tr => tr.Uid == uid);

        return tagRequest == null ? null : MapToBo(tagRequest);
    }

    public async Task<IReadOnlyList<TagRequestBo>> GetByRequesterAsync(Guid requestedByUid)
    {
        return await context.TagRequests
            .AsNoTracking()
            .Where(tr => tr.RequestedByUid == requestedByUid)
            .OrderByDescending(tr => tr.CreatedAt)
            .Select(tr => MapToBo(tr))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TagRequestBo>> GetByStatusAsync(string status)
    {
        return await context.TagRequests
            .AsNoTracking()
            .Where(tr => tr.Status == status)
            .OrderByDescending(tr => tr.CreatedAt)
            .Select(tr => MapToBo(tr))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetApprovedNamesAsync()
    {
        return await context.TagRequests
            .AsNoTracking()
            .Where(tr => tr.Status == "Approved")
            .OrderBy(tr => tr.Name)
            .Select(tr => tr.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await context.TagRequests
            .AnyAsync(tr => tr.Name == name);
    }

    public async Task<TagRequestBo> UpdateAsync(TagRequestBo tagRequestBo)
    {
        var tagRequest = await context.TagRequests
            .FirstOrDefaultAsync(tr => tr.Uid == tagRequestBo.Uid);

        if (tagRequest == null)
        {
            throw new KeyNotFoundException($"TagRequest with UID {tagRequestBo.Uid} not found.");
        }

        tagRequest.Status = tagRequestBo.Status;
        tagRequest.ReviewedByUid = tagRequestBo.ReviewedByUid;
        tagRequest.ReviewedAt = tagRequestBo.ReviewedAt;

        await context.SaveChangesAsync();

        return MapToBo(tagRequest);
    }

    private static TagRequestBo MapToBo(TagRequest tagRequest) => new(
        tagRequest.Uid,
        tagRequest.RequestedByUid,
        tagRequest.Name,
        tagRequest.Status,
        tagRequest.ReviewedByUid,
        tagRequest.ReviewedAt,
        tagRequest.CreatedAt
    );
}
