IF EXISTS (
    SELECT 1
    FROM sys.key_constraints
    WHERE name = N'UQ__User__49A14740D988F0F5'
      AND parent_object_id = OBJECT_ID(N'dbo.[User]')
)
BEGIN
ALTER TABLE dbo.[User] DROP CONSTRAINT [UQ__User__49A14740D988F0F5];
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.[User]')
      AND name = N'UX_User_EmailAddress_NotNull'
)
BEGIN
CREATE UNIQUE INDEX [UX_User_EmailAddress_NotNull]
    ON dbo.[User]([EmailAddress])
    WHERE [EmailAddress] IS NOT NULL;
END
