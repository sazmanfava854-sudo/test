-- دسترسی Windows User به HRPerformanceDB (برای IIS App Pool Identity)
-- در SSMS با sysadmin اجرا کنید — LOGIN_NAME را با Identity واقعی App Pool عوض کنید
-- مثال: ITC\sys-hoseine-sh

USE [master];
GO

DECLARE @LoginName NVARCHAR(256) = N'ITC\sys-hoseine-sh';

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @LoginName)
BEGIN
    DECLARE @sql NVARCHAR(500) = N'CREATE LOGIN [' + REPLACE(@LoginName, N'\', N'\\') + N'] FROM WINDOWS;';
    EXEC sp_executesql @sql;
    PRINT N'Login ساخته شد: ' + @LoginName;
END
ELSE
    PRINT N'Login از قبل وجود دارد: ' + @LoginName;
GO

USE [HRPerformanceDB];
GO

DECLARE @LoginName NVARCHAR(256) = N'ITC\sys-hoseine-sh';
DECLARE @UserName NVARCHAR(256) = @LoginName;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @UserName)
BEGIN
    DECLARE @sql NVARCHAR(500) = N'CREATE USER [' + REPLACE(@UserName, N'\', N'\\') + N'] FOR LOGIN [' + REPLACE(@LoginName, N'\', N'\\') + N'];';
    EXEC sp_executesql @sql;
    PRINT N'User ساخته شد.';
END

DECLARE @roleSql NVARCHAR(500) = N'ALTER ROLE db_datareader ADD MEMBER [' + REPLACE(@LoginName, N'\', N'\\') + N'];';
EXEC sp_executesql @roleSql;
SET @roleSql = N'ALTER ROLE db_datawriter ADD MEMBER [' + REPLACE(@LoginName, N'\', N'\\') + N'];';
EXEC sp_executesql @roleSql;

PRINT N'دسترسی db_datareader/db_datawriter داده شد.';
GO
