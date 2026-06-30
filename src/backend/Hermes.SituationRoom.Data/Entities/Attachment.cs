using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Attachment
{
    public Guid Uid { get; set; }

    public Guid OwnerUid { get; set; }

    public string FileName { get; set; }

    public string ContentType { get; set; }

    public long SizeBytes { get; set; }

    public string StorageKey { get; set; }

    public byte[] EncryptionIv { get; set; }

    public byte[] EncryptionTag { get; set; }

    public byte[] WrappedKey { get; set; }

    public byte[] ContentHash { get; set; }

    public Guid? PostUid { get; set; }

    public Guid? MessageUid { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User OwnerU { get; set; }

    public virtual Post PostU { get; set; }

    public virtual Message MessageU { get; set; }
}
