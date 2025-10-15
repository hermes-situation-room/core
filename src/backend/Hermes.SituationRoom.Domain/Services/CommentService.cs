namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public class CommentService(ICommentRepository commentRepository) : ICommentService
{
    public Task<IReadOnlyList<CommentBo>> GetPostCommentsAsync(Guid postUid) =>
        commentRepository.GetPostCommentsAsync(postUid);
    
    public Task<Guid> CreateCommentAsync(CreateCommentDto createCommentDto) =>
        commentRepository.AddAsync(MapToBo(createCommentDto, DateTime.UtcNow));

    public Task<CommentBo> UpdateCommentAsync(CommentBo updatedComment) => commentRepository.UpdateAsync(updatedComment);

    public Task DeleteCommentAsync(Guid commentUid) =>
        commentRepository.DeleteAsync(commentUid);

    private static CommentBo MapToBo(CreateCommentDto createCommentDto, DateTime timestamp) => new(createCommentDto.Uid,
        timestamp,
        createCommentDto.CreatorUid,
        createCommentDto.PostUid,
        createCommentDto.Content
    );
}
