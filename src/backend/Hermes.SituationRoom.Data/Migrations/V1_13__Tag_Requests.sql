-- ========================
-- Migration V1.13: Tag Requests
-- Description: Adds a TagRequest table so users can request new post tags
--              that admins approve or reject (§2.2.4.3 Expand/Request Post Tags)
-- ========================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TagRequest' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TagRequest
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        RequestedByUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_TagRequest_RequestedBy FOREIGN KEY REFERENCES dbo.[User](UID),
        Name NVARCHAR(64) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        ReviewedByUID UNIQUEIDENTIFIER NULL
            CONSTRAINT FK_TagRequest_ReviewedBy FOREIGN KEY REFERENCES dbo.[User](UID),
        ReviewedAt DATETIME NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    -- Indexes
    CREATE INDEX IX_TagRequest_Status ON dbo.TagRequest(Status);
    CREATE INDEX IX_TagRequest_RequestedBy ON dbo.TagRequest(RequestedByUID);
    CREATE INDEX IX_TagRequest_CreatedAt ON dbo.TagRequest(CreatedAt DESC);
    CREATE UNIQUE INDEX UQ_TagRequest_Name ON dbo.TagRequest(Name);
END
GO
