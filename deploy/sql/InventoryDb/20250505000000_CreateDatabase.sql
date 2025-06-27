USE [Master]
GO

-- Create InventoryDb if not exists
IF DB_ID('InventoryDb') IS NULL
BEGIN
    CREATE DATABASE [InventoryDb];
    PRINT 'InventoryDb database created successfully.';
END
ELSE
BEGIN
    PRINT 'InventoryDb database already exists.';
END
