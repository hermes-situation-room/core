-- ========================
-- Migration V1.11: Feature Extensions
-- Date: October 17, 2025
-- Description: Adds support for Post Likes, Comment Threading, Extended Journalist Profiles, Reports, and User Tags
-- ========================

-- ========================
-- 1. Post Likes
-- ========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PostLike' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PostLike
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        PostUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_PostLike_Post FOREIGN KEY REFERENCES dbo.Post(UID) ON DELETE CASCADE,
        UserUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_PostLike_User FOREIGN KEY REFERENCES dbo.[User](UID) ON DELETE NO ACTION,
        Timestamp DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_PostLike_User_Post UNIQUE(PostUID, UserUID)
    );

    -- Indexes for performance
    CREATE INDEX IX_PostLike_Post ON dbo.PostLike(PostUID);
    CREATE INDEX IX_PostLike_User ON dbo.PostLike(UserUID);
    CREATE INDEX IX_PostLike_Timestamp ON dbo.PostLike(Timestamp DESC);
END
GO

-- ========================
-- 2. Comment Threading (Replies)
-- ========================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comment') AND name = 'ParentCommentUID')
BEGIN
    ALTER TABLE dbo.Comment
    ADD ParentCommentUID UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comment_ParentComment')
BEGIN
    ALTER TABLE dbo.Comment
    ADD CONSTRAINT FK_Comment_ParentComment FOREIGN KEY (ParentCommentUID) REFERENCES dbo.Comment(UID);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comment_Parent' AND object_id = OBJECT_ID('dbo.Comment'))
BEGIN
    CREATE INDEX IX_Comment_Parent ON dbo.Comment(ParentCommentUID);
END
GO

-- ========================
-- 3. Extended Journalist Profiles
-- ========================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.[User]') AND name = 'Bio')
BEGIN
    ALTER TABLE dbo.[User] ADD Bio NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.[User]') AND name = 'PublishedWorkLink')
BEGIN
    ALTER TABLE dbo.[User] ADD PublishedWorkLink NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.[User]') AND name = 'LinkedInUrl')
BEGIN
    ALTER TABLE dbo.[User] ADD LinkedInUrl NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.[User]') AND name = 'TwitterUrl')
BEGIN
    ALTER TABLE dbo.[User] ADD TwitterUrl NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.[User]') AND name = 'WebsiteUrl')
BEGIN
    ALTER TABLE dbo.[User] ADD WebsiteUrl NVARCHAR(500) NULL;
END
GO

-- Create SpecializationTag table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SpecializationTag' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.SpecializationTag
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Create JournalistSpecialization junction table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'JournalistSpecialization' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.JournalistSpecialization
    (
        JournalistUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_JournalistSpec_Journalist FOREIGN KEY REFERENCES dbo.Journalist(UserUID) ON DELETE CASCADE,
        SpecializationTagUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_JournalistSpec_Tag FOREIGN KEY REFERENCES dbo.SpecializationTag(UID) ON DELETE CASCADE,
        PRIMARY KEY (JournalistUID, SpecializationTagUID)
    );

    -- Index for lookups
    CREATE INDEX IX_JournalistSpec_Journalist ON dbo.JournalistSpecialization(JournalistUID);
    CREATE INDEX IX_JournalistSpec_Tag ON dbo.JournalistSpecialization(SpecializationTagUID);
END
GO

-- ========================
-- 4. Content Moderation (Reports)
-- ========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Report' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Report
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ReporterUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Report_Reporter FOREIGN KEY REFERENCES dbo.[User](UID),
        TargetType NVARCHAR(50) NOT NULL,
        TargetUID UNIQUEIDENTIFIER NOT NULL,
        Reason NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
        Timestamp DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ReviewedByUID UNIQUEIDENTIFIER NULL
            CONSTRAINT FK_Report_Reviewer FOREIGN KEY REFERENCES dbo.[User](UID),
        ReviewedAt DATETIME NULL,
        AdminNotes NVARCHAR(MAX) NULL
    );

    -- Indexes
    CREATE INDEX IX_Report_Status ON dbo.Report(Status);
    CREATE INDEX IX_Report_TargetType ON dbo.Report(TargetType, TargetUID);
    CREATE INDEX IX_Report_Reporter ON dbo.Report(ReporterUID);
    CREATE INDEX IX_Report_Timestamp ON dbo.Report(Timestamp DESC);
END
GO

-- ========================
-- 5. Private User Tags (Contact Organization)
-- ========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserTag' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.UserTag
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OwnerUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_UserTag_Owner FOREIGN KEY REFERENCES dbo.[User](UID) ON DELETE CASCADE,
        TaggedUserUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_UserTag_Tagged FOREIGN KEY REFERENCES dbo.[User](UID),
        TagName NVARCHAR(100) NOT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_UserTag_Owner_Tagged_Name UNIQUE(OwnerUID, TaggedUserUID, TagName)
    );

    -- Indexes
    CREATE INDEX IX_UserTag_Owner ON dbo.UserTag(OwnerUID);
    CREATE INDEX IX_UserTag_Tagged ON dbo.UserTag(TaggedUserUID);
    CREATE INDEX IX_UserTag_TagName ON dbo.UserTag(OwnerUID, TagName);
END
GO

-- ========================
-- 6. Notifications
-- ========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notification' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Notification
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Notification_User FOREIGN KEY REFERENCES dbo.[User](UID) ON DELETE CASCADE,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(500) NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        RelatedEntityUID UNIQUEIDENTIFIER NULL,
        RelatedEntityType NVARCHAR(50) NULL
    );

    -- Indexes
    CREATE INDEX IX_Notification_User_Read ON dbo.Notification(UserUID, IsRead);
    CREATE INDEX IX_Notification_Created ON dbo.Notification(CreatedAt DESC);
    CREATE INDEX IX_Notification_Type ON dbo.Notification(Type);
END
GO

-- ========================
-- 7. Insert default specialization tags (only if table is empty)
-- ========================
IF NOT EXISTS (SELECT * FROM dbo.SpecializationTag)
BEGIN
    INSERT INTO dbo.SpecializationTag (UID, Name) VALUES
    (NEWID(), 'Investigative Journalism'),
    (NEWID(), 'Political Reporting'),
    (NEWID(), 'Human Rights'),
    (NEWID(), 'Environmental Issues'),
    (NEWID(), 'Social Justice'),
    (NEWID(), 'War Correspondence'),
    (NEWID(), 'Data Journalism'),
    (NEWID(), 'Local News'),
    (NEWID(), 'International Affairs'),
    (NEWID(), 'Crime & Justice'),
    (NEWID(), 'Health & Science'),
    (NEWID(), 'Business & Economy');
END
GO
