IF OBJECT_ID(N'dbo.ROLE', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ROLE]
    (
        Id          BIGINT PRIMARY KEY IDENTITY(1, 1),
        Name        NVARCHAR(50) NOT NULL,
        Description NVARCHAR(200) NULL,
        IsActive    BIT NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        DeletedAt   DATETIME2 NULL
    );
END;