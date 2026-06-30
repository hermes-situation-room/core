-- ========================
-- Migration V1.15: Tag Catalog Table
-- Description: Introduces a DB-backed Tag catalog table as the single source of
--              truth for which post tags exist (replacing the C# Tag enum).
--              Seeds the 16 canonical tag names plus any already-approved
--              TagRequest names. PostTag keeps storing the tag NAME string, so
--              existing post rows are untouched.
-- ========================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tag' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Tag
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(64) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE UNIQUE INDEX UQ_Tag_Name ON dbo.Tag(Name);
END
GO

-- Seed the canonical tag names (idempotent: only insert names not already present).
INSERT INTO dbo.Tag (Name)
SELECT seed.Name
FROM (VALUES
    ('HUMAN_RIGHTS'),
    ('ENVIRONMENT'),
    ('CORRUPTION'),
    ('ELECTIONS'),
    ('CONFLICT'),
    ('FREEDOM_OF_SPEECH'),
    ('MIGRATION'),
    ('POLICE_VIOLENCE'),
    ('CORPORATE_ACCOUNTABILITY'),
    ('SOCIAL_JUSTICE'),
    ('HEALTHCARE'),
    ('EDUCATION'),
    ('LGBTQ_RIGHTS'),
    ('GENDER_EQUALITY'),
    ('TECHNOLOGY_PRIVACY'),
    ('TRANSPARENCY')
) AS seed(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Tag t WHERE t.Name = seed.Name);
GO

-- Seed any already-approved tag requests so existing approvals become usable.
INSERT INTO dbo.Tag (Name)
SELECT DISTINCT tr.Name
FROM dbo.TagRequest tr
WHERE tr.Status = 'Approved'
  AND NOT EXISTS (SELECT 1 FROM dbo.Tag t WHERE t.Name = tr.Name);
GO
