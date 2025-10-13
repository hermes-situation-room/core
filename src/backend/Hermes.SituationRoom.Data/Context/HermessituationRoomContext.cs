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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activist>(entity =>
        {
            entity.HasKey(e => e.UserUid).HasName("PK__Activist__A1F26A8AE52A2890");

            entity.ToTable("Activist");

            entity.HasIndex(e => e.Username, "UQ__Activist__536C85E44AD7BE57").IsUnique();

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
            entity.HasKey(e => e.Uid).HasName("PK__Chat__C5B196024D3043C8");

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
            entity.HasKey(e => e.Uid).HasName("PK__Comment__C5B19602C1E462E3");

            entity.ToTable("Comment");

            entity.HasIndex(e => new { e.PostUid, e.Timestamp }, "IX_Comment_Post_Timestamp");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatorUid).HasColumnName("CreatorUID");
            entity.Property(e => e.PostUid).HasColumnName("PostUID");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.CreatorU).WithMany(p => p.Comments)
                .HasForeignKey(d => d.CreatorUid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_User");

            entity.HasOne(d => d.PostU).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostUid)
                .HasConstraintName("FK_Comment_Post");
        });

        modelBuilder.Entity<Journalist>(entity =>
        {
            entity.HasKey(e => e.UserUid).HasName("PK__Journali__A1F26A8A9BB2D199");

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
            entity.HasKey(e => e.Uid).HasName("PK__Message__C5B19602E4AD44B2");

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
            entity.HasKey(e => e.Uid).HasName("PK__Post__C5B19602C6E1D452");

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
            entity.HasKey(e => e.Uid).HasName("PK__PrivacyL__C5B1960259B21AC7");

            entity.ToTable("PrivacyLevelPersonal");

            entity.HasIndex(e => e.ConsumerUid, "IX_PrivacyLevelPersonal_Consumer");

            entity.HasIndex(e => e.OwnerUid, "IX_PrivacyLevelPersonal_Owner");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("UID");
            entity.Property(e => e.ConsumerUid).HasColumnName("ConsumerUID");
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
            entity.HasKey(e => e.Uid).HasName("PK__User__C5B196026CADD503");

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
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
