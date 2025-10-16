namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public class CommentService(ICommentRepository commentRepository, IMapper mapper) : ICommentService
{
    public async Task<IReadOnlyList<CommentDto>> GetPostCommentsAsync(Guid postUid)
    {
        var comments = await commentRepository.GetPostCommentsAsync(postUid);
        return mapper.Map<IReadOnlyList<CommentDto>>(comments);
    }
    
    public async Task<Guid> CreateCommentAsync(CreateCommentRequestDto createCommentDto)
    {
        var commentBo = mapper.Map<CommentBo>(createCommentDto);
        commentBo = commentBo with { Timestamp = DateTime.UtcNow };
        return await commentRepository.AddAsync(commentBo);
    }

    public async Task<CommentDto> UpdateCommentAsync(UpdateCommentRequestDto updateCommentDto)
    {
        var existingComment = await commentRepository.GetCommentAsync(updateCommentDto.Uid);
        var commentBo = existingComment with { Uid = updateCommentDto.Uid, Content = updateCommentDto.Content };
        var updatedComment = await commentRepository.UpdateAsync(commentBo);
        return mapper.Map<CommentDto>(updatedComment);
    }

    public Task DeleteCommentAsync(Guid commentUid) =>
        commentRepository.DeleteAsync(commentUid);
}
