IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_PERMISSION_Name'
    AND object_id = OBJECT_ID(N'dbo.[PERMISSION]')
)
BEGIN
    CREATE UNIQUE INDEX UX_PERMISSION_Name
    ON [dbo].[PERMISSION](Name)
    WHERE DeletedAt IS NULL;
END;