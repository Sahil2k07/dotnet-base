IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_ROLE_Name'
    AND object_id = OBJECT_ID(N'dbo.[ROLE]')
)
BEGIN
    CREATE UNIQUE INDEX UX_ROLE_Name
    ON [dbo].[ROLE](Name)
    WHERE DeletedAt IS NULL;
END;