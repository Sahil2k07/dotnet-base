IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_ROLE_PERMISSION_RoleId_PermissionId'
    AND object_id = OBJECT_ID(N'dbo.[ROLE_PERMISSION]')
)
BEGIN
    CREATE UNIQUE INDEX UX_ROLE_PERMISSION_RoleId_PermissionId
    ON [dbo].[ROLE_PERMISSION](RoleId, PermissionId)
    WHERE DeletedAt IS NULL;
END;