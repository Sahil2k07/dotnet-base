MERGE INTO [dbo].[ROLE] AS Target
USING
(
    VALUES
        (N'SUPER_ADMIN', N'Full system access'),
        (N'ADMIN',       N'Administrative access'),
        (N'MANAGER',     N'Manager access'),
        (N'USER',        N'Default user role')
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