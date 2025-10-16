using AutoMapper;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects;

namespace Hermes.SituationRoom.Api.Profiles;

public class SituationRoomProfile : Profile
{
    public SituationRoomProfile()
    {
        // Activist mappings
        CreateMap<ActivistBo, ActivistDto>();
        CreateMap<ActivistDto, ActivistBo>();
        CreateMap<CreateActivistRequestDto, ActivistBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.Password,
                null,
                null,
                src.FirstName,
                src.LastName,
                src.EmailAddress,
                src.UserName,
                src.IsFirstNameVisible,
                src.IsLastNameVisible,
                src.IsEmailVisible
            ));
        CreateMap<UpdateActivistRequestDto, ActivistBo>()
            .ConstructUsing(src => new(
                src.Uid,
                null,
                null,
                null,
                src.FirstName,
                src.LastName,
                src.EmailAddress,
                src.UserName,
                src.IsFirstNameVisible,
                src.IsLastNameVisible,
                src.IsEmailVisible
            ));

        // Journalist mappings
        CreateMap<JournalistBo, JournalistDto>();
        CreateMap<JournalistDto, JournalistBo>();
        CreateMap<CreateJournalistRequestDto, JournalistBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.Password,
                null,
                null,
                src.FirstName,
                src.LastName,
                src.EmailAddress,
                src.Employer
            ));
        CreateMap<UpdateJournalistRequestDto, JournalistBo>()
            .ConstructUsing(src => new(
                src.Uid,
                null,
                null,
                null,
                src.FirstName,
                src.LastName,
                src.EmailAddress,
                src.Employer
            ));

        // User mappings
        CreateMap<UserBo, UserDto>();
        CreateMap<UserDto, UserBo>();
        CreateMap<CreateUserRequestDto, UserBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.Password,
                null,
                null,
                src.FirstName,
                src.LastName,
                src.EmailAddress
            ));
        CreateMap<UpdateUserRequestDto, UserBo>()
            .ConstructUsing(src => new(
                src.Uid,
                null,
                null,
                null,
                src.FirstName,
                src.LastName,
                src.EmailAddress
            ));

        // Post mappings
        CreateMap<PostWithTagsBo, PostWithTagsDto>();
        CreateMap<PostWithTagsDto, PostWithTagsBo>();
        CreateMap<CreatePostRequestDto, PostWithTagsBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                DateTime.UtcNow,
                src.Title,
                src.Description,
                src.Content,                
                src.CreatorUid,
                src.PrivacyLevel,
                src.Tags
            ));
        CreateMap<UpdatePostRequestDto, PostWithTagsBo>()
            .ConstructUsing(src => new(
                src.Uid,
                DateTime.UtcNow,
                src.Title,
                src.Description,
                src.Content,
                Guid.Empty,
                src.PrivacyLevel,
                src.Tags
            ));

        // Comment mappings
        CreateMap<CommentBo, CommentDto>();
        CreateMap<CommentDto, CommentBo>();
        CreateMap<CreateCommentRequestDto, CommentBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                DateTime.UtcNow,
                src.CreatorUid,
                src.PostUid,
                src.Content
            ));
        CreateMap<UpdateCommentRequestDto, CommentBo>()
            .ConstructUsing(src => new(
                src.Uid,
                DateTime.UtcNow,
                Guid.Empty,
                Guid.Empty,
                src.Content
            ));

        // Chat mappings
        CreateMap<ChatBo, ChatDto>();
        CreateMap<ChatDto, ChatBo>();
        CreateMap<CreateChatRequestDto, ChatBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.User1Uid,
                src.User2Uid
            ));

        // Message mappings
        CreateMap<MessageBo, MessageDto>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Uid ?? Guid.Empty));
        CreateMap<MessageDto, MessageBo>();
        CreateMap<CreateMessageRequestDto, MessageBo>()
            .ConstructUsing(src => new(
                src.Content,
                src.SenderUid,
                src.ChatUid,
                DateTime.UtcNow
            ));
        CreateMap<UpdateMessageRequestDto, MessageBo>()
            .ConstructUsing(src => new(
                src.Content,
                Guid.Empty,
                Guid.Empty,
                DateTime.UtcNow
            ) { Uid = src.Uid });

        // PrivacyLevelPersonal mappings
        CreateMap<PrivacyLevelPersonalBo, PrivacyLevelPersonalDto>();
        CreateMap<PrivacyLevelPersonalDto, PrivacyLevelPersonalBo>();
        CreateMap<CreatePrivacyLevelPersonalRequestDto, PrivacyLevelPersonalBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.IsFirstNameVisible,
                src.IsLastNameVisible,
                src.IsEmailVisible,
                src.OwnerUid,
                src.ConsumerUid
            ));
        CreateMap<UpdatePrivacyLevelPersonalRequestDto, PrivacyLevelPersonalBo>()
            .ConstructUsing(src => new(
                src.Uid,
                src.IsFirstNameVisible,
                src.IsLastNameVisible,
                src.IsEmailVisible,
                Guid.Empty,
                Guid.Empty
            ));

        // Current User mappings
        CreateMap<CurrentUserBo, CurrentUserDto>();
        CreateMap<CurrentUserDto, CurrentUserBo>();

        // User Profile mappings
        CreateMap<UserProfileBo, UserProfileDto>();
        CreateMap<UserProfileDto, UserProfileBo>();

        // Login mappings
        CreateMap<LoginActivistRequestDto, LoginActivistBo>();
        CreateMap<LoginJournalistRequestDto, LoginJournalistBo>();
    }
}