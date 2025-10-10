namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface ICommentService
{
    Task<IReadOnlyList<CommentBo>> GetPostCommentsAsync(Guid postUid);
    
    Task<Guid> CreateCommentAsync(CreateCommentDto createCommentBo);

    Task<CommentBo> UpdateCommentAsync(CommentBo updatedComment);

    Task DeleteCommentAsync(Guid commentUid);
}
