DECLARE @ConstraintName NVARCHAR(200);

SELECT @ConstraintName = kc.name
FROM sys.key_constraints kc
         JOIN sys.tables t ON kc.parent_object_id = t.object_id
         JOIN sys.columns c ON c.object_id = t.object_id
         JOIN sys.index_columns ic ON ic.object_id = t.object_id AND ic.column_id = c.column_id
WHERE t.name = 'User'
  AND kc.type_desc = 'UNIQUE_CONSTRAINT'
  AND c.name = 'EmailAddress';

IF @ConstraintName IS NOT NULL
BEGIN
EXEC('ALTER TABLE dbo.[User] DROP CONSTRAINT [' + @ConstraintName + ']');
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
