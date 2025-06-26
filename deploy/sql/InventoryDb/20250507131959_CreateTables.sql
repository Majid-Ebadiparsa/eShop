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
    WHERE [MigrationId] = N'20250507131959_Init'
)
BEGIN
    CREATE TABLE [InventoryItems] (
        [Id] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_InventoryItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250507131959_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ProductId', N'Quantity') AND [object_id] = OBJECT_ID(N'[InventoryItems]'))
        SET IDENTITY_INSERT [InventoryItems] ON;
    EXEC(N'INSERT INTO [InventoryItems] ([Id], [ProductId], [Quantity])
    VALUES (''3fa85f64-5717-4562-b3fc-2c963f66afa6'', ''11111111-1111-1111-1111-111111111111'', 100)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ProductId', N'Quantity') AND [object_id] = OBJECT_ID(N'[InventoryItems]'))
        SET IDENTITY_INSERT [InventoryItems] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250507131959_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250507131959_Init', N'9.0.4');
END;

COMMIT;
GO

