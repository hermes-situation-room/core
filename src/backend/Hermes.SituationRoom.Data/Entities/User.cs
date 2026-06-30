using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class User
{
    public Guid Uid { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string EmailAddress { get; set; }

    public byte[] PasswordHash { get; set; }

    public byte[] PasswordSalt { get; set; }

    public string ProfileIcon { get; set; }

    public string ProfileIconColor { get; set; }

    public string Bio { get; set; }

    public string PublishedWorkLink { get; set; }

    public string LinkedInUrl { get; set; }

    public string TwitterUrl { get; set; }

    public string WebsiteUrl { get; set; }

    public bool IsAdmin { get; set; }

    public virtual Activist Activist { get; set; }

    public virtual ICollection<Chat> ChatUser1Us { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatUser2Us { get; set; } = new List<Chat>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Journalist Journalist { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<PrivacyLevelPersonal> PrivacyLevelPersonalConsumerUs { get; set; } = new List<PrivacyLevelPersonal>();

    public virtual ICollection<PrivacyLevelPersonal> PrivacyLevelPersonalOwnerUs { get; set; } = new List<PrivacyLevelPersonal>();

    public virtual ICollection<UserChatReadStatus> UserChatReadStatuses { get; set; } = new List<UserChatReadStatus>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual ICollection<Report> ReportsSubmitted { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportsReviewed { get; set; } = new List<Report>();

    public virtual ICollection<UserTag> UserTagsOwned { get; set; } = new List<UserTag>();

    public virtual ICollection<UserTag> UserTagsTagged { get; set; } = new List<UserTag>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<TagRequest> TagRequestsSubmitted { get; set; } = new List<TagRequest>();

    public virtual ICollection<TagRequest> TagRequestsReviewed { get; set; } = new List<TagRequest>();
}
