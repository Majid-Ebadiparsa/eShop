-- Create OrderDb if not exists
IF DB_ID('OrderDb') IS NULL
BEGIN
    CREATE DATABASE [OrderDb];
    PRINT 'OrderDb database created successfully.';
END
ELSE
BEGIN
    PRINT 'OrderDb database already exists.';
END
