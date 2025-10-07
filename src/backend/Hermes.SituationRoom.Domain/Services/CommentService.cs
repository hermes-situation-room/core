namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface ICommentService
{
    Task<IReadOnlyList<CommentBo>> GetPostCommentsAsync(Guid postUid);
    
    Task<Guid> CreateCommentAsync(CreateCommentDto createCommentBo);

    Task<CommentBo> UpdateCommentAsync(CommentBo updatedComment);

    Task DeleteCommentAsync(Guid commentUid);
}

public class CommentService(ICommentRepository commentRepository) : ICommentService
{
    public Task<IReadOnlyList<CommentBo>> GetPostCommentsAsync(Guid postUid) =>
        commentRepository.GetPostCommentsAsync(postUid);
    
    public Task<Guid> CreateCommentAsync(CreateCommentDto createCommentDto) =>
        commentRepository.AddAsync(MapToBo(createCommentDto, DateTime.Now));

    public Task<CommentBo> UpdateCommentAsync(CommentBo updatedComment) => commentRepository.Update(updatedComment);

    public Task DeleteCommentAsync(Guid commentUid)
    {
        commentRepository.Delete(commentUid);
        return Task.CompletedTask;
    }

    private static CommentBo MapToBo(CreateCommentDto createCommentDto, DateTime timestamp) => new(createCommentDto.Uid,
        timestamp,
        createCommentDto.CreatorUid,
        createCommentDto.PostUid,
        createCommentDto.Content
    );
}
