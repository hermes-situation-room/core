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
                src.ProfileIcon,
                src.ProfileIconColor,
                src.UserName,
                src.IsFirstNameVisible,
                src.IsLastNameVisible,
                src.IsEmailVisible,
                false
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
                src.ProfileIcon,
                src.ProfileIconColor,
                src.UserName,
                src.IsFirstNameVisible,
                src.IsLastNameVisible,
                src.IsEmailVisible,
                false
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
                src.ProfileIcon,
                src.ProfileIconColor,
                src.Employer,
                null,
                null,
                null,
                null,
                null,
                null
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
                src.ProfileIcon,
                src.ProfileIconColor,
                src.Employer,
                null,
                null,
                null,
                null,
                null,
                null
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
                src.EmailAddress,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                false
            ));
        CreateMap<UpdateUserRequestDto, UserBo>()
            .ConstructUsing(src => new(
                src.Uid,
                null,
                null,
                null,
                src.FirstName,
                src.LastName,
                src.EmailAddress,
                src.ProfileIcon,
                src.ProfileIconColor,
                null,
                null,
                null,
                null,
                null,
                false
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
                null,
                null,
                src.Content,
                src.ParentCommentUid
            ));
        CreateMap<UpdateCommentRequestDto, CommentBo>()
            .ConstructUsing(src => new(
                src.Uid,
                DateTime.UtcNow,
                Guid.Empty,
                Guid.Empty,
                null,
                null,
                src.Content,
                null
            ));

        // Chat mappings
        CreateMap<ChatBo, ChatDto>();
        CreateMap<ChatDto, ChatBo>();
        CreateMap<CreateChatRequestDto, ChatBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.MemberUids != null && src.MemberUids.Count > 0 ? src.MemberUids[0] : src.User1Uid,
                src.MemberUids != null && src.MemberUids.Count > 1 ? src.MemberUids[1] : src.User2Uid,
                src.MemberUids != null && src.MemberUids.Count > 0
                    ? src.MemberUids
                    : new[] { src.User1Uid, src.User2Uid },
                src.Name,
                src.MemberUids != null && src.MemberUids.Count > 2
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
            )
            {
                ExpiresAt = src.DisappearAfterSeconds.HasValue
                    ? DateTime.UtcNow.AddSeconds(src.DisappearAfterSeconds.Value)
                    : (DateTime?)null,
                IsViewOnce = src.IsViewOnce
            });
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

        // PostLike mappings
        CreateMap<PostLikeBo, PostLikeDto>();
        CreateMap<PostLikeDto, PostLikeBo>();
        CreateMap<CreatePostLikeDto, PostLikeBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.PostUid,
                src.UserUid,
                DateTime.UtcNow
            ));

        // UserTag mappings
        CreateMap<UserTagBo, UserTagDto>();
        CreateMap<UserTagDto, UserTagBo>();
        CreateMap<CreateUserTagDto, UserTagBo>()
            .ConstructUsing(src => new(
                Guid.NewGuid(),
                src.OwnerUid,
                src.TaggedUserUid,
                src.TagName,
                src.Notes,
                DateTime.UtcNow
            ));

        // Report mappings
        CreateMap<Data.Entities.Report, ReportBo>()
            .ConstructUsing(src => new(
                src.Uid,
                src.ReporterUid,
                src.TargetType,
                src.TargetUid,
                src.Reason,
                src.Status,
                src.Timestamp,
                src.ReviewedByUid,
                src.ReviewedAt,
                src.AdminNotes
            ));
        CreateMap<ReportBo, Data.Entities.Report>()
            .ForMember(dest => dest.ReporterU, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedByU, opt => opt.Ignore());
        CreateMap<ReportBo, ReportDto>();
        CreateMap<ReportDto, ReportBo>();

        // TagRequest mappings
        CreateMap<TagRequestBo, TagRequestDto>();
        CreateMap<TagRequestDto, TagRequestBo>();

        // ContentFilterRule mappings
        CreateMap<ContentFilterRuleBo, ContentFilterRuleDto>();
        CreateMap<ContentFilterRuleDto, ContentFilterRuleBo>();

        // Attachment mappings (DTO never exposes key material)
        CreateMap<AttachmentBo, AttachmentDto>();
    }
}