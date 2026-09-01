IF OBJECT_ID(N'dbo.USER_PROFILE', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[USER_PROFILE] 
    (
        Id             BIGINT PRIMARY KEY IDENTITY(1, 1),
        UserId         BIGINT NOT NULL,
        FirstName      NVARCHAR(50) NOT NULL,
        LastName       NVARCHAR(50) NOT NULL,
        Phone          NVARCHAR(20) NULL,
        DisplayName    NVARCHAR(100) NULL,
        DateOfBirth    DATE NULL,
        ProfilePicture NVARCHAR(500) NULL,
        CreatedAt      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        DeletedAt      DATETIME2 NULL,

        CONSTRAINT FK_USER_PROFILE_USER FOREIGN KEY (UserId)
        REFERENCES [dbo].[USER](Id) ON DELETE CASCADE
    )
END;