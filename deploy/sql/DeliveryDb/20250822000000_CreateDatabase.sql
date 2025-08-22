IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DeliveryDb')
BEGIN
  CREATE DATABASE [DeliveryDb];
END
GO
