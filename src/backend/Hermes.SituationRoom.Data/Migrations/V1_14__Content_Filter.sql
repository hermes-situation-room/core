-- ========================
-- Migration V1.14: Content Filter
-- Description: Adds a ContentFilterRule table so admins can define patterns
--              that redact or block content in public posts and comments
--              (§2.2.5.4 Content Filter). Never applied to private messages.
-- ========================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContentFilterRule' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ContentFilterRule
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Pattern NVARCHAR(256) NOT NULL,
        Action NVARCHAR(50) NOT NULL DEFAULT 'Redact',
        Enabled BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    -- Indexes
    CREATE INDEX IX_ContentFilterRule_Enabled ON dbo.ContentFilterRule(Enabled);
    CREATE INDEX IX_ContentFilterRule_CreatedAt ON dbo.ContentFilterRule(CreatedAt DESC);
    CREATE UNIQUE INDEX UQ_ContentFilterRule_Pattern ON dbo.ContentFilterRule(Pattern);
END
GO
