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
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());
        CreateMap<UpdateActivistRequestDto, ActivistBo>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

        // Journalist mappings
        CreateMap<JournalistBo, JournalistDto>();
        CreateMap<JournalistDto, JournalistBo>();
        CreateMap<CreateJournalistRequestDto, JournalistBo>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());
        CreateMap<UpdateJournalistRequestDto, JournalistBo>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

        // User mappings
        CreateMap<UserBo, UserDto>();
        CreateMap<UserDto, UserBo>();
        CreateMap<CreateUserRequestDto, UserBo>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());
        CreateMap<UpdateUserRequestDto, UserBo>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

        // Post mappings
        CreateMap<PostWithTagsBo, PostWithTagsDto>();
        CreateMap<PostWithTagsDto, PostWithTagsBo>();
        CreateMap<CreatePostRequestDto, PostWithTagsBo>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore());
        CreateMap<UpdatePostRequestDto, PostWithTagsBo>()
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorUid, opt => opt.Ignore());

        // Comment mappings
        CreateMap<CommentBo, CommentDto>();
        CreateMap<CommentDto, CommentBo>();
        CreateMap<CreateCommentRequestDto, CommentBo>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore());
        CreateMap<UpdateCommentRequestDto, CommentBo>()
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorUid, opt => opt.Ignore())
            .ForMember(dest => dest.PostUid, opt => opt.Ignore());

        // Chat mappings
        CreateMap<ChatBo, ChatDto>();
        CreateMap<ChatDto, ChatBo>();
        CreateMap<CreateChatRequestDto, ChatBo>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => Guid.NewGuid()));

        // Message mappings
        CreateMap<MessageBo, MessageDto>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Uid ?? Guid.Empty));
        CreateMap<MessageDto, MessageBo>();
        CreateMap<CreateMessageRequestDto, MessageBo>()
            .ForMember(dest => dest.Uid, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore());
        CreateMap<UpdateMessageRequestDto, MessageBo>()
            .ForMember(dest => dest.SenderUid, opt => opt.Ignore())
            .ForMember(dest => dest.ChatUid, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore());

        // PrivacyLevelPersonal mappings
        CreateMap<PrivacyLevelPersonalBo, PrivacyLevelPersonalDto>();
        CreateMap<PrivacyLevelPersonalDto, PrivacyLevelPersonalBo>();
        CreateMap<CreatePrivacyLevelPersonalRequestDto, PrivacyLevelPersonalBo>()
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => Guid.NewGuid()));
        CreateMap<UpdatePrivacyLevelPersonalRequestDto, PrivacyLevelPersonalBo>()
            .ForMember(dest => dest.OwnerUid, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerUid, opt => opt.Ignore());

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
