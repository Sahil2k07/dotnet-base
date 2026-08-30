IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_User_Email'
    AND object_id = OBJECT_ID(N'dbo.[USER]')
)
BEGIN
    CREATE UNIQUE INDEX UX_User_Email
    ON [dbo].[USER] (Email)
    WHERE DeletedAt IS NULL;
END;