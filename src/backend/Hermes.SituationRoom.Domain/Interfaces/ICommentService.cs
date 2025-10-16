namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> GetPostCommentsAsync(Guid postUid);
    
    Task<Guid> CreateCommentAsync(CreateCommentRequestDto createCommentDto);

    Task<CommentDto> UpdateCommentAsync(UpdateCommentRequestDto updateCommentDto);

    Task DeleteCommentAsync(Guid commentUid);
}
