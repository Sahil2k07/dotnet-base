MERGE INTO [dbo].[PERMISSION] AS Target
USING
(
    VALUES
        (N'role.read',       N'View roles'),
        (N'role.create',     N'Create roles'),
        (N'role.update',     N'Update roles'),
        (N'role.delete',     N'Delete roles'),

        (N'permission.read', N'View permissions')
) AS Source (Name, Description)
ON Target.Name = Source.Name
AND Target.DeletedAt IS NULL
WHEN NOT MATCHED 
BY TARGET THEN
INSERT
(
    Name,
    Description
)
VALUES
(
    Source.Name,
    Source.Description
);