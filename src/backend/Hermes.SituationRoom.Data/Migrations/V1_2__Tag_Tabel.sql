-- ========================
-- PosTag Tabel
-- ========================
CREATE TABLE dbo.PostTag
(
    PostUID UNIQUEIDENTIFIER NOT NULL,
    Tag NVARCHAR(64) NOT NULL,
    CONSTRAINT PK_PostTag PRIMARY KEY (PostUID, Tag),
    CONSTRAINT FK_PostTag_Post FOREIGN KEY (PostUID)
        REFERENCES dbo.Post(UID) ON DELETE CASCADE
);

-- ========================
-- Indexes for performance
-- ========================
CREATE INDEX IX_PostTag_Tag ON dbo.PostTag (Tag);
