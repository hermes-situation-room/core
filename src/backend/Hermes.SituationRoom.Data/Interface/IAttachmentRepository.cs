namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IAttachmentRepository
{
    Task<Guid> AddAsync(AttachmentBo attachmentBo);
    Task<AttachmentBo?> GetAsync(Guid uid);
    Task<IReadOnlyList<AttachmentBo>> GetByPostAsync(Guid postUid);
    Task<IReadOnlyList<AttachmentBo>> GetByMessageAsync(Guid messageUid);
    Task LinkToPostAsync(Guid attachmentUid, Guid ownerUid, Guid postUid);
    Task LinkToMessageAsync(Guid attachmentUid, Guid ownerUid, Guid messageUid);
    Task DeleteAsync(Guid uid);
}
