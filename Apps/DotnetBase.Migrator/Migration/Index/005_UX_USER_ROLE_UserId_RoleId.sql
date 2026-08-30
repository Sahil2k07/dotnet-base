IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_USER_ROLE_UserId_RoleId'
    AND object_id = OBJECT_ID(N'dbo.[USER_ROLE]')
)
BEGIN
    CREATE UNIQUE INDEX UX_USER_ROLE_UserId_RoleId
    ON [dbo].[USER_ROLE](UserId, RoleId)
    WHERE DeletedAt IS NULL;
END;