namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public class CommentService(ICommentRepository commentRepository) : ICommentService
{
    public async Task<IReadOnlyList<CommentBo>> GetPostCommentsAsync(Guid postUid)
    {
        if (postUid == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty.", nameof(postUid));

        return await commentRepository.GetPostCommentsAsync(postUid);
    }
    
    public async Task<Guid> CreateCommentAsync(CreateCommentDto createCommentDto)
    {
        ArgumentNullException.ThrowIfNull(createCommentDto, nameof(createCommentDto));

        if (createCommentDto.PostUid == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty.", nameof(createCommentDto.PostUid));

        if (createCommentDto.CreatorUid == Guid.Empty)
            throw new ArgumentException("Creator ID cannot be empty.", nameof(createCommentDto.CreatorUid));

        if (string.IsNullOrWhiteSpace(createCommentDto.Content))
            throw new BusinessValidationException(nameof(createCommentDto.Content), "Comment content is required.");

        if (createCommentDto.Content.Length > 1000)
            throw new BusinessValidationException(nameof(createCommentDto.Content), "Comment content cannot exceed 1000 characters.");

        return await commentRepository.AddAsync(MapToBo(createCommentDto, DateTime.UtcNow));
    }

    public async Task<CommentBo> UpdateCommentAsync(CommentBo updatedComment)
    {
        ArgumentNullException.ThrowIfNull(updatedComment, nameof(updatedComment));

        if (updatedComment.Uid == Guid.Empty)
            throw new ArgumentException("Comment ID cannot be empty.", nameof(updatedComment.Uid));

        if (string.IsNullOrWhiteSpace(updatedComment.Content))
            throw new BusinessValidationException(nameof(updatedComment.Content), "Comment content is required.");

        if (updatedComment.Content.Length > 1000)
            throw new BusinessValidationException(nameof(updatedComment.Content), "Comment content cannot exceed 1000 characters.");

        return await commentRepository.UpdateAsync(updatedComment);
    }

    public async Task DeleteCommentAsync(Guid commentUid)
    {
        if (commentUid == Guid.Empty)
            throw new ArgumentException("Comment ID cannot be empty.", nameof(commentUid));

        await commentRepository.DeleteAsync(commentUid);
    }

    private static CommentBo MapToBo(CreateCommentDto createCommentDto, DateTime timestamp) => new(createCommentDto.Uid,
        timestamp,
        createCommentDto.CreatorUid,
        createCommentDto.PostUid,
        createCommentDto.Content
    );
}
