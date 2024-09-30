USE [master]
DROP DATABASE IF EXISTS [perpetuumsa]
GO

RESTORE DATABASE [perpetuumsa] FROM  DISK = N'/var/opt/mssql/data/perpetuumsa.bak' WITH  FILE = 1,  MOVE N'perpetuumsa' TO N'/var/opt/mssql/data/psa.mdf',  MOVE N'perpetuumsa_log' TO N'/var/opt/mssql/data/psa.ldf',  NOUNLOAD,  REPLACE, STATS = 4,  BUFFERCOUNT = 64, MAXTRANSFERSIZE = 4194304
ALTER DATABASE [perpetuumsa] SET MULTI_USER
GO
ALTER DATABASE [perpetuumsa] SET COMPATIBILITY_LEVEL = 130
GO
