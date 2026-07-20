-- SQL Authentication — اگر Windows Auth (ITC\sys-hoseine-sh) ممکن نیست
-- در SSMS با sysadmin روی 172.16.10.232 اجرا کنید
-- سپس در appsettings.Production.json از User Id=hr_app استفاده کنید

USE [master];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'hr_app')
BEGIN
    CREATE LOGIN [hr_app] WITH PASSWORD = N'ChangeMe_Strong_Password_123!';
    PRINT N'Login hr_app ساخته شد — رمز را عوض کنید';
END
GO

USE [HRPerformanceDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'hr_app')
    CREATE USER [hr_app] FOR LOGIN [hr_app];
GO

ALTER ROLE db_datareader ADD MEMBER [hr_app];
ALTER ROLE db_datawriter ADD MEMBER [hr_app];
GO

PRINT N'OK — hr_app آماده است';
GO
