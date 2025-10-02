-- ========================
-- User base table
-- ========================
CREATE TABLE dbo.[User]
(
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Password NVARCHAR(255) NOT NULL,
    -- hashed/salted password, not MAX
    FirstName NVARCHAR(255) NOT NULL,
    LastName NVARCHAR(255) NOT NULL,
    EmailAddress NVARCHAR(255) NOT NULL UNIQUE
);

-- ========================
-- Journalist (subtype of User)
-- ========================
CREATE TABLE dbo.Journalist
(
    UserUID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
        CONSTRAINT FK_Journalist_User FOREIGN KEY REFERENCES dbo.[User](UID),
    Employer NVARCHAR(255) NOT NULL
);

-- ========================
-- Activist (subtype of User)
-- ========================
CREATE TABLE dbo.Activist
(
    UserUID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
        CONSTRAINT FK_Activist_User FOREIGN KEY REFERENCES dbo.[User](UID),
    Username NVARCHAR(255) NOT NULL UNIQUE,
    IsFirstNameVisible BIT NOT NULL,
    IsLastNameVisible BIT NOT NULL,
    IsEmailVisible BIT NOT NULL
);

-- ========================
-- Chat
-- ========================
CREATE TABLE dbo.Chat
(
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    User1UID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_Chat_User1 FOREIGN KEY REFERENCES dbo.[User](UID),
    User2UID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_Chat_User2 FOREIGN KEY REFERENCES dbo.[User](UID)
);

-- ========================
-- Message
-- ========================
CREATE TABLE dbo.[Message]
(
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Timestamp] DATETIME NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    SenderUID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_Message_User FOREIGN KEY REFERENCES dbo.[User](UID),
    ChatUID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_Message_Chat FOREIGN KEY REFERENCES dbo.Chat(UID)
);

-- ========================
-- Privacy Levels (per-user relationship)
-- ========================
CREATE TABLE dbo.PrivacyLevelPersonal
(
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    IsFirstNameVisible BIT NOT NULL,
    IsLastNameVisible BIT NOT NULL,
    IsEmailVisible BIT NOT NULL,
    OwnerUID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_PrivacyLevelPersonal_Owner FOREIGN KEY REFERENCES dbo.[User](UID),
    ConsumerUID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_PrivacyLevelPersonal_Consumer FOREIGN KEY REFERENCES dbo.[User](UID)
);

-- ========================
-- Post
-- ========================
CREATE TABLE dbo.Post
(
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Timestamp] DATETIME NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatorUID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_Post_User FOREIGN KEY REFERENCES dbo.[User](UID)
);

-- ========================
-- Comment
-- ========================
CREATE TABLE dbo.Comment
(
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Timestamp] DATETIME NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatorUID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_Comment_User FOREIGN KEY REFERENCES dbo.[User](UID),
    PostUID UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_Comment_Post FOREIGN KEY REFERENCES dbo.Post(UID)
);

-- ========================
-- Indexes for performance
-- ========================
-- Message queries by chat
CREATE INDEX IX_Message_Chat_Timestamp ON dbo.[Message](ChatUID, [Timestamp]);

-- Messages by sender
CREATE INDEX IX_Message_Sender ON dbo.[Message](SenderUID);

-- Posts by creator
CREATE INDEX IX_Post_Creator_Timestamp ON dbo.Post(CreatorUID, [Timestamp]);

-- Comments per post
CREATE INDEX IX_Comment_Post_Timestamp ON dbo.Comment(PostUID, [Timestamp]);

-- Chat participants
CREATE INDEX IX_Chat_User1 ON dbo.Chat(User1UID);
CREATE INDEX IX_Chat_User2 ON dbo.Chat(User2UID);

-- Privacy lookups
CREATE INDEX IX_PrivacyLevelPersonal_Owner ON dbo.PrivacyLevelPersonal(OwnerUID);
CREATE INDEX IX_PrivacyLevelPersonal_Consumer ON dbo.PrivacyLevelPersonal(ConsumerUID);