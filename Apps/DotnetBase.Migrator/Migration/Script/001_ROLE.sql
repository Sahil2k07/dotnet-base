MERGE INTO [dbo].[ROLE] AS Target
USING
(
    VALUES
        (N'SuperAdmin', N'Full system access'),
        (N'Admin',      N'Administrative access'),
        (N'Manager',    N'Manager access'),
        (N'User',       N'Default user role')
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