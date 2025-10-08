namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class CommentRepository(IHermessituationRoomContext context) : ICommentRepository
{
    public async Task<Guid> AddAsync(CommentBo commentBo)
    {
        ArgumentNullException.ThrowIfNull(commentBo);

        var newComment = new Comment
        {
            Uid = Guid.NewGuid(),
            Timestamp = commentBo.Timestamp,
            CreatorUid = commentBo.CreatorUid,
            PostUid = commentBo.PostUid,
            Content = commentBo.Content,
        };

        context.Comments.Add(newComment);
        await context.SaveChangesAsync();

        return newComment.Uid;
    }

    public async Task<IReadOnlyList<CommentBo>> GetPostCommentsAsync(Guid postUid) => await context.Comments
        .AsNoTracking()
        .Where(c => c.PostUid == postUid)
        .OrderByDescending(c => c.Timestamp)
        .Select(c => MapToBo(c))
        .ToListAsync();

    public async Task<CommentBo> UpdateAsync(CommentBo updatedComment)
    {
        ArgumentNullException.ThrowIfNull(updatedComment);
        if (updatedComment.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedComment));

        var comment = await context.Comments.FirstOrDefaultAsync(u => u.Uid == updatedComment.Uid)
                      ?? throw new KeyNotFoundException($"Comment with UID {updatedComment.Uid} was not found.");

        comment.Content = updatedComment.Content;

        context.Comments.Update(comment);
        await context.SaveChangesAsync();

        return MapToBo(comment);
    }

    public Task DeleteAsync(Guid commentId)
    {
        if (commentId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(commentId));

        var comment = context.Comments.FirstOrDefault(u => u.Uid == commentId);
        if (comment is null)
            throw new KeyNotFoundException($"Comment with GUID {commentId} was not found.");

        context.Comments.Remove(comment);
        context.SaveChanges();

        return Task.CompletedTask;
    }

    private static CommentBo MapToBo(Comment comment) => new(comment.Uid,
        comment.Timestamp,
        comment.CreatorUid,
        comment.PostUid,
        comment.Content
    );
}
