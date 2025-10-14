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

    public virtual Activist Activist { get; set; }

    public virtual ICollection<Chat> ChatUser1Us { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatUser2Us { get; set; } = new List<Chat>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Journalist Journalist { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<PrivacyLevelPersonal> PrivacyLevelPersonalConsumerUs { get; set; } = new List<PrivacyLevelPersonal>();

    public virtual ICollection<PrivacyLevelPersonal> PrivacyLevelPersonalOwnerUs { get; set; } = new List<PrivacyLevelPersonal>();
}
