namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface IPostService
{
    Task<PostBo> GetPostAsync(Guid postUid);
    
    Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid);

    Task<IReadOnlyList<PostBo>> GetPostsAsync();

    Task<Guid> CreatePostAsync(CreatePostDto createPostBo);

    Task<PostBo> UpdatePostAsync(PostBo updatedPost);

    Task DeletePostAsync(Guid postUid);
}

public class PostService(IPostRepository postRepository) : IPostService
{
    public Task<PostBo> GetPostAsync(Guid postUid) => postRepository.GetPostBoAsync(postUid);

    public Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid) => postRepository.GetUserPostsAsync(userUid);
    
    public Task<IReadOnlyList<PostBo>> GetAllActivistPostsAsync() => postRepository.GetAllActivistPostsAsync();
    
    public Task<IReadOnlyList<PostBo>> GetAllJournalistPostsAsync() => postRepository.GetAllJournalistPostsAsync();

    public Task<IReadOnlyList<PostBo>> GetPostsAsync() => postRepository.GetAllPostBosAsync();

    public Task<Guid> CreatePostAsync(CreatePostDto createPostDto) =>
        postRepository.AddAsync(MapToBo(createPostDto, DateTime.Now));

    public Task<PostBo> UpdatePostAsync(PostBo updatedPost) => postRepository.UpdateAsync(updatedPost);

    public Task DeletePostAsync(Guid postUid)
    {
        postRepository.DeleteAsync(postUid);
        return Task.CompletedTask;
    }

    private static PostBo MapToBo(CreatePostDto createPostDto, DateTime timestamp) => new(createPostDto.Uid,
        timestamp,
        createPostDto.Title,
        createPostDto.Description,
        createPostDto.Content,
        createPostDto.CreatorUid
    );
}
