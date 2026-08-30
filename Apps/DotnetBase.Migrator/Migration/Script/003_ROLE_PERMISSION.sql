MERGE INTO [dbo].[ROLE_PERMISSION] AS Target
USING
(
    VALUES
        -- SUPER_ADMIN
        (N'SUPER_ADMIN', N'role.read'),
        (N'SUPER_ADMIN', N'role.create'),
        (N'SUPER_ADMIN', N'role.update'),
        (N'SUPER_ADMIN', N'role.delete'),
        (N'SUPER_ADMIN', N'permission.read'),

        -- ADMIN
        (N'ADMIN', N'role.read'),
        (N'ADMIN', N'role.create'),
        (N'ADMIN', N'role.update'),
        (N'ADMIN', N'role.delete'),
        (N'ADMIN', N'permission.read')
) AS Source (RoleName, PermissionName)
INNER JOIN [dbo].[ROLE] R
    ON R.Name = Source.RoleName
    AND R.DeletedAt IS NULL
INNER JOIN [dbo].[PERMISSION] P
    ON P.Name = Source.PermissionName
    AND P.DeletedAt IS NULL
ON Target.RoleId = R.Id
AND Target.PermissionId = P.Id
AND Target.DeletedAt IS NULL
WHEN NOT MATCHED BY TARGET THEN
INSERT
(
    RoleId,
    PermissionId
)
VALUES
(
    R.Id,
    P.Id
);