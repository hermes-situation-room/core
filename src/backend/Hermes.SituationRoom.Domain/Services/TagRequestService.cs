namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public sealed class TagRequestService(
    ITagRequestRepository tagRequestRepository,
    ITagRepository tagRepository,
    IMapper mapper) : ITagRequestService
{
    private const string StatusPending = "Pending";
    private const string StatusApproved = "Approved";
    private const string StatusRejected = "Rejected";

    public async Task<TagRequestDto> CreateTagRequestAsync(Guid requestedByUid, string name)
    {
        var normalizedName = NormalizeName(name);

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ValidationBusinessException(nameof(name), "Tag name must not be empty.");
        }

        if (await tagRepository.ExistsByNameAsync(normalizedName))
        {
            throw new ValidationBusinessException(nameof(name), $"Tag '{normalizedName}' already exists.");
        }

        if (await tagRequestRepository.ExistsByNameAsync(normalizedName))
        {
            throw new ValidationBusinessException(nameof(name), $"A request for tag '{normalizedName}' already exists.");
        }

        var tagRequest = new TagRequestBo(
            Guid.Empty,
            requestedByUid,
            normalizedName,
            StatusPending,
            null,
            null,
            DateTime.UtcNow
        );

        var uid = await tagRequestRepository.AddAsync(tagRequest);
        var created = await tagRequestRepository.GetByIdAsync(uid);

        return mapper.Map<TagRequestDto>(created);
    }

    public async Task<IReadOnlyList<TagRequestDto>> GetMyTagRequestsAsync(Guid requestedByUid)
    {
        var requests = await tagRequestRepository.GetByRequesterAsync(requestedByUid);
        return mapper.Map<IReadOnlyList<TagRequestDto>>(requests);
    }

    public async Task<IReadOnlyList<TagRequestDto>> GetPendingTagRequestsAsync()
    {
        var requests = await tagRequestRepository.GetByStatusAsync(StatusPending);
        return mapper.Map<IReadOnlyList<TagRequestDto>>(requests);
    }

    public async Task<TagRequestDto> ApproveTagRequestAsync(Guid uid, Guid reviewerUid)
    {
        var tagRequest = await GetPendingOrThrowAsync(uid);

        var approved = tagRequest with
        {
            Status = StatusApproved,
            ReviewedByUid = reviewerUid,
            ReviewedAt = DateTime.UtcNow
        };

        var updated = await tagRequestRepository.UpdateAsync(approved);

        // Inserting the approved name into the Tag catalog (idempotent) makes it
        // immediately selectable and persistable for posts.
        await tagRepository.AddTagAsync(approved.Name);

        return mapper.Map<TagRequestDto>(updated);
    }

    public async Task<TagRequestDto> RejectTagRequestAsync(Guid uid, Guid reviewerUid)
    {
        var tagRequest = await GetPendingOrThrowAsync(uid);

        var rejected = tagRequest with
        {
            Status = StatusRejected,
            ReviewedByUid = reviewerUid,
            ReviewedAt = DateTime.UtcNow
        };

        var updated = await tagRequestRepository.UpdateAsync(rejected);
        return mapper.Map<TagRequestDto>(updated);
    }

    private async Task<TagRequestBo> GetPendingOrThrowAsync(Guid uid)
    {
        var tagRequest = await tagRequestRepository.GetByIdAsync(uid);

        if (tagRequest == null)
        {
            throw new KeyNotFoundException($"TagRequest with UID {uid} not found.");
        }

        if (tagRequest.Status != StatusPending)
        {
            throw new ValidationBusinessException(nameof(uid), $"Tag request has already been {tagRequest.Status.ToLowerInvariant()}.");
        }

        return tagRequest;
    }

    private static string NormalizeName(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : name.Trim().ToUpperInvariant().Replace(' ', '_');
}
