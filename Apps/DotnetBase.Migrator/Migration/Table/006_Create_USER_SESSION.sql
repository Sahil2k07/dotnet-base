IF OBJECT_ID(N'dbo.USER_SESSION', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[USER_SESSION] 
    (
        Id               BIGINT PRIMARY KEY IDENTITY(1, 1),
        UserId           BIGINT NOT NULL,
        UserRoleId       BIGINT NOT NULL,
        DisplayId        UNIQUEIDENTIFIER NOT NULL,
        SessionTokenHash NVARCHAR(128) NOT NULL,
        CreatedAt        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ExpiresAt        DATETIME2 NOT NULL,
        LastUsedAt       DATETIME2 NULL,
        RevokedAt        DATETIME2 NULL,
        DeletedAt        DATETIME2 NULL,

        CONSTRAINT FK_USER_SESSION_USER FOREIGN KEY (UserId)
        REFERENCES [dbo].[USER](Id) ON DELETE CASCADE,

        CONSTRAINT FK_USER_SESSION_USER_ROLE FOREIGN KEY (UserRoleId)
        REFERENCES [dbo].[USER_ROLE](Id) ON DELETE NO ACTION
    )
END;