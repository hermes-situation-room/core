DECLARE @ConstraintName NVARCHAR(200);

SELECT @ConstraintName = name
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('dbo.PrivacyLevelPersonal')
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.PrivacyLevelPersonal'), 'IsFirstNameVisible', 'ColumnId');

IF @ConstraintName IS NOT NULL
BEGIN
EXEC('ALTER TABLE dbo.[PrivacyLevelPersonal] DROP CONSTRAINT [' + @ConstraintName + ']');
END

ALTER TABLE dbo.[PrivacyLevelPersonal]
    ADD CONSTRAINT DF_PrivacyLevelPersonal_IsFirstNameVisible DEFAULT NULL FOR IsFirstNameVisible;


DECLARE @ConstraintName2 NVARCHAR(200);

SELECT @ConstraintName2 = name
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('dbo.PrivacyLevelPersonal')
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.PrivacyLevelPersonal'), 'IsLastNameVisible', 'ColumnId');

IF @ConstraintName2 IS NOT NULL
BEGIN
EXEC('ALTER TABLE dbo.[PrivacyLevelPersonal] DROP CONSTRAINT [' + @ConstraintName2 + ']');
END

ALTER TABLE dbo.[PrivacyLevelPersonal]
    ADD CONSTRAINT DF_PrivacyLevelPersonal_IsLastNameVisible DEFAULT NULL FOR IsLastNameVisible;


DECLARE @ConstraintName3 NVARCHAR(200);

SELECT @ConstraintName3 = name
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('dbo.PrivacyLevelPersonal')
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.PrivacyLevelPersonal'), 'IsEmailVisible', 'ColumnId');

IF @ConstraintName3 IS NOT NULL
BEGIN
EXEC('ALTER TABLE dbo.[PrivacyLevelPersonal] DROP CONSTRAINT [' + @ConstraintName3 + ']');
END

ALTER TABLE dbo.[PrivacyLevelPersonal]
    ADD CONSTRAINT DF_PrivacyLevelPersonal_IsEmailVisible DEFAULT NULL FOR IsEmailVisible;