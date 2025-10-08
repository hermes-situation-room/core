#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface ICommentRepository
{
    Task<Guid> AddAsync(CommentBo commentBo);

    Task<IReadOnlyList<CommentBo>> GetPostCommentsAsync(Guid postUid);

    Task<CommentBo> UpdateAsync(CommentBo updatedComment);

    Task DeleteAsync(Guid commentUid);
}
