IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104180848_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceHealthHistories] (
        [Id] int NOT NULL IDENTITY,
        [ServiceName] nvarchar(450) NOT NULL,
        [IsHealthy] bit NOT NULL,
        [StatusMessage] nvarchar(max) NULL,
        [ResponseTimeMs] bigint NOT NULL,
        [CheckedAt] datetime2 NOT NULL,
        [ErrorCode] nvarchar(50) NULL,
        [ExceptionType] nvarchar(200) NULL,
        [StackTrace] nvarchar(4000) NULL,
        CONSTRAINT [PK_ServiceHealthHistories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104180848_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceHealthStatuses] (
        [Id] int NOT NULL IDENTITY,
        [ServiceName] nvarchar(450) NOT NULL,
        [IsHealthy] bit NOT NULL,
        [StatusMessage] nvarchar(max) NULL,
        [ResponseTimeMs] bigint NOT NULL,
        [CheckedAt] datetime2 NOT NULL,
        [ErrorCode] nvarchar(50) NULL,
        [ExceptionType] nvarchar(200) NULL,
        [StackTrace] nvarchar(4000) NULL,
        CONSTRAINT [PK_ServiceHealthStatuses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104180848_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceHealthHistories_ServiceName_CheckedAt] ON [ServiceHealthHistories] ([ServiceName], [CheckedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104180848_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceHealthStatuses_ServiceName] ON [ServiceHealthStatuses] ([ServiceName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104180848_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260104180848_InitialCreate', N'10.0.1');
END;

COMMIT;
GO

