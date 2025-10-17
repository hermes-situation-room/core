using System;
using System.Collections.Generic;
using Hermes.SituationRoom.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hermes.SituationRoom.Data.Context;

public partial class HermessituationRoomContext : DbContext
{
    public HermessituationRoomContext(DbContextOptions<HermessituationRoomContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activist> Activists { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Journalist> Journalists { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostTag> PostTags { get; set; }

    public virtual DbSet<PrivacyLevelPersonal> PrivacyLevelPersonals { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserChatReadStatus> UserChatReadStatuses { get; set; }

    public virtual DbSet<PostLike> PostLikes { get; set; }

    public virtual DbSet<SpecializationTag> SpecializationTags { get; set; }

    public virtual DbSet<JournalistSpecialization> JournalistSpecializations { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<UserTag> UserTags { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activist>(entity =>
        {
            entity.HasKey(e => e.UserUid).HasName("PK__Activist__A1F26A8A453D4393");

            entity.ToTable("Activist");

            entity.HasIndex(e => e.Username, "UQ__Activist__536C85E4A246408A").IsUnique();

            entity.Property(e => e.UserUid)
                .ValueGeneratedNever()
                .HasColumnName("UserUID");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.UserU).WithOne(p => p.Activist)
                .HasForeignKey<Activist>(d => d.UserUid)
                .HasConstraintName("FK_Activist_User");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__Chat__C5B19602D8F85B3E");

            entity.ToTable("Chat");

            entity.HasIndex(e => e.User1Uid, "IX_Chat_User1");

            entity.HasIndex(e => e.User2Uid, "IX_Chat_User2");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.User1Uid).HasColumnName("User1UID");
            entity.Property(e => e.User2Uid).HasColumnName("User2UID");

            entity.HasOne(d => d.User1U).WithMany(p => p.ChatUser1Us)
                .HasForeignKey(d => d.User1Uid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_User1");

            entity.HasOne(d => d.User2U).WithMany(p => p.ChatUser2Us)
                .HasForeignKey(d => d.User2Uid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_User2");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__Comment__C5B19602390A6664");

            entity.ToTable("Comment");

            entity.HasIndex(e => new { e.PostUid, e.Timestamp }, "IX_Comment_Post_Timestamp");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatorUid).HasColumnName("CreatorUID");
            entity.Property(e => e.PostUid).HasColumnName("PostUID");
            entity.Property(e => e.ParentCommentUid).HasColumnName("ParentCommentUID");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.CreatorU).WithMany(p => p.Comments)
                .HasForeignKey(d => d.CreatorUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_User");

            entity.HasOne(d => d.PostU).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostUid)
                .HasConstraintName("FK_Comment_Post");

            entity.HasOne(d => d.ParentCommentU).WithMany(p => p.Replies)
                .HasForeignKey(d => d.ParentCommentUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_ParentComment");
        });

        modelBuilder.Entity<Journalist>(entity =>
        {
            entity.HasKey(e => e.UserUid).HasName("PK__Journali__A1F26A8A8EFFB2B5");

            entity.ToTable("Journalist");

            entity.Property(e => e.UserUid)
                .ValueGeneratedNever()
                .HasColumnName("UserUID");
            entity.Property(e => e.Employer)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.UserU).WithOne(p => p.Journalist)
                .HasForeignKey<Journalist>(d => d.UserUid)
                .HasConstraintName("FK_Journalist_User");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__Message__C5B1960260E12629");

            entity.ToTable("Message");

            entity.HasIndex(e => new { e.ChatUid, e.Timestamp }, "IX_Message_Chat_Timestamp");

            entity.HasIndex(e => e.SenderUid, "IX_Message_Sender");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.ChatUid).HasColumnName("ChatUID");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SenderUid).HasColumnName("SenderUID");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.ChatU).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatUid)
                .HasConstraintName("FK_Message_Chat");

            entity.HasOne(d => d.SenderU).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_User");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__Post__C5B196026519AB55");

            entity.ToTable("Post");

            entity.HasIndex(e => new { e.CreatorUid, e.Timestamp }, "IX_Post_Creator_Timestamp");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatorUid).HasColumnName("CreatorUID");
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.CreatorU).WithMany(p => p.Posts)
                .HasForeignKey(d => d.CreatorUid)
                .HasConstraintName("FK_Post_User");
        });

        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasKey(e => new { e.PostUid, e.Tag });

            entity.ToTable("PostTag");

            entity.HasIndex(e => e.Tag, "IX_PostTag_Tag");

            entity.Property(e => e.PostUid).HasColumnName("PostUID");
            entity.Property(e => e.Tag).HasMaxLength(64);

            entity.HasOne(d => d.PostU).WithMany(p => p.PostTags)
                .HasForeignKey(d => d.PostUid)
                .HasConstraintName("FK_PostTag_Post");
        });

        modelBuilder.Entity<PrivacyLevelPersonal>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__PrivacyL__C5B196026FB36187");

            entity.ToTable("PrivacyLevelPersonal");

            entity.HasIndex(e => e.ConsumerUid, "IX_PrivacyLevelPersonal_Consumer");

            entity.HasIndex(e => e.OwnerUid, "IX_PrivacyLevelPersonal_Owner");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.ConsumerUid).HasColumnName("ConsumerUID");
            entity.Property(e => e.IsEmailVisible).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IsFirstNameVisible).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IsLastNameVisible).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.OwnerUid).HasColumnName("OwnerUID");

            entity.HasOne(d => d.ConsumerU).WithMany(p => p.PrivacyLevelPersonalConsumerUs)
                .HasForeignKey(d => d.ConsumerUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrivacyLevelPersonal_Consumer");

            entity.HasOne(d => d.OwnerU).WithMany(p => p.PrivacyLevelPersonalOwnerUs)
                .HasForeignKey(d => d.OwnerUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrivacyLevelPersonal_Owner");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__User__C5B19602F47A1223");

            entity.ToTable("User");

            entity.HasIndex(e => e.EmailAddress, "UX_User_EmailAddress_NotNull")
                .IsUnique()
                .HasFilter("([EmailAddress] IS NOT NULL)");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.EmailAddress).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.PasswordSalt)
                .IsRequired()
                .HasMaxLength(16);
            entity.Property(e => e.ProfileIcon)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("User");
            entity.Property(e => e.ProfileIconColor)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("#3b82f6");
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.PublishedWorkLink).HasMaxLength(500);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(500);
            entity.Property(e => e.TwitterUrl).HasMaxLength(500);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<UserChatReadStatus>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__UserChat__C5B196021B4E652C");

            entity.ToTable("UserChatReadStatus");

            entity.HasIndex(e => e.ChatId, "IX_UserChatReadStatus_Chat");

            entity.HasIndex(e => e.UserId, "IX_UserChatReadStatus_User");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.ChatId).HasColumnName("ChatID");
            entity.Property(e => e.ReadTime).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Chat).WithMany(p => p.UserChatReadStatuses)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("FK_UserChatReadStatus_Chat");

            entity.HasOne(d => d.User).WithMany(p => p.UserChatReadStatuses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserChatReadStatus_User");
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__PostLike__C5B19602");

            entity.ToTable("PostLike");

            entity.HasIndex(e => e.PostUid, "IX_PostLike_Post");
            entity.HasIndex(e => e.UserUid, "IX_PostLike_User");
            entity.HasIndex(e => e.Timestamp, "IX_PostLike_Timestamp");
            entity.HasIndex(e => new { e.PostUid, e.UserUid }, "UQ_PostLike_User_Post").IsUnique();

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.PostUid).HasColumnName("PostUID");
            entity.Property(e => e.UserUid).HasColumnName("UserUID");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.PostU).WithMany(p => p.PostLikes)
                .HasForeignKey(d => d.PostUid)
                .HasConstraintName("FK_PostLike_Post");

            entity.HasOne(d => d.UserU).WithMany(p => p.PostLikes)
                .HasForeignKey(d => d.UserUid)
                .HasConstraintName("FK_PostLike_User");
        });

        modelBuilder.Entity<SpecializationTag>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__SpecializationTag__C5B19602");

            entity.ToTable("SpecializationTag");

            entity.HasIndex(e => e.Name, "UQ__SpecializationTag__Name").IsUnique();

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<JournalistSpecialization>(entity =>
        {
            entity.HasKey(e => new { e.JournalistUid, e.SpecializationTagUid })
                .HasName("PK__JournalistSpecialization");

            entity.ToTable("JournalistSpecialization");

            entity.HasIndex(e => e.JournalistUid, "IX_JournalistSpec_Journalist");
            entity.HasIndex(e => e.SpecializationTagUid, "IX_JournalistSpec_Tag");

            entity.Property(e => e.JournalistUid).HasColumnName("JournalistUID");
            entity.Property(e => e.SpecializationTagUid).HasColumnName("SpecializationTagUID");

            entity.HasOne(d => d.JournalistU).WithMany(p => p.JournalistSpecializations)
                .HasForeignKey(d => d.JournalistUid)
                .HasConstraintName("FK_JournalistSpec_Journalist");

            entity.HasOne(d => d.SpecializationTagU).WithMany(p => p.JournalistSpecializations)
                .HasForeignKey(d => d.SpecializationTagUid)
                .HasConstraintName("FK_JournalistSpec_Tag");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__Report__C5B19602");

            entity.ToTable("Report");

            entity.HasIndex(e => e.Status, "IX_Report_Status");
            entity.HasIndex(e => new { e.TargetType, e.TargetUid }, "IX_Report_TargetType");
            entity.HasIndex(e => e.ReporterUid, "IX_Report_Reporter");
            entity.HasIndex(e => e.Timestamp, "IX_Report_Timestamp");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.ReporterUid).HasColumnName("ReporterUID");
            entity.Property(e => e.TargetType)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.TargetUid).HasColumnName("TargetUID");
            entity.Property(e => e.Reason).IsRequired();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("PENDING");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
            entity.Property(e => e.ReviewedByUid).HasColumnName("ReviewedByUID");
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ReporterU).WithMany(p => p.ReportsSubmitted)
                .HasForeignKey(d => d.ReporterUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Report_Reporter");

            entity.HasOne(d => d.ReviewedByU).WithMany(p => p.ReportsReviewed)
                .HasForeignKey(d => d.ReviewedByUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Report_Reviewer");
        });

        modelBuilder.Entity<UserTag>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__UserTag__C5B19602");

            entity.ToTable("UserTag");

            entity.HasIndex(e => e.OwnerUid, "IX_UserTag_Owner");
            entity.HasIndex(e => e.TaggedUserUid, "IX_UserTag_Tagged");
            entity.HasIndex(e => new { e.OwnerUid, e.TagName }, "IX_UserTag_TagName");
            entity.HasIndex(e => new { e.OwnerUid, e.TaggedUserUid, e.TagName }, "UQ_UserTag_Owner_Tagged_Name").IsUnique();

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.OwnerUid).HasColumnName("OwnerUID");
            entity.Property(e => e.TaggedUserUid).HasColumnName("TaggedUserUID");
            entity.Property(e => e.TagName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.OwnerU).WithMany(p => p.UserTagsOwned)
                .HasForeignKey(d => d.OwnerUid)
                .HasConstraintName("FK_UserTag_Owner");

            entity.HasOne(d => d.TaggedUserU).WithMany(p => p.UserTagsTagged)
                .HasForeignKey(d => d.TaggedUserUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserTag_Tagged");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__Notification__C5B19602");

            entity.ToTable("Notification");

            entity.HasIndex(e => new { e.UserUid, e.IsRead }, "IX_Notification_User_Read");
            entity.HasIndex(e => e.CreatedAt, "IX_Notification_Created");
            entity.HasIndex(e => e.Type, "IX_Notification_Type");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.UserUid).HasColumnName("UserUID");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.RelatedEntityUid).HasColumnName("RelatedEntityUID");
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);

            entity.HasOne(d => d.UserU).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserUid)
                .HasConstraintName("FK_Notification_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
