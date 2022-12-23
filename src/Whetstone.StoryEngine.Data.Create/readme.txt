
dotnet add package Microsoft.EntityFrameworkCore.Tools.DotNet --version 2.1.0-preview1-final

Use this to install the latest .net tool
dotnet tool install --global dotnet-ef 

Microsoft.EntityFrameworkCore.Tools --version 2.1.2

dotnet tool install Microsoft.EntityFrameworkCore.Tools.DotNet --version 2.1.0-preview1-final -g

dotnet tool update <PACKAGE_NAME> <-g|--global> [--configfile] [--framework] [-v|--verbosity]

dotnet ef migrations add UserContextBootstrap -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create

dotnet ef migrations add SmsMessageHistory -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create


dotnet ef migrations add ClientBodyLogging -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create 

dotnet ef migrations add SessionErrorUpdate -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create 


REM When recreating the intial script, remove the UserDataContextModelSnapShot.cs file. The creation process references this file to determine differences.
dotnet ef migrations add InitialCreate -c UserDataContext   --startup-project ..\Whetstone.StoryEngine.Data.Create 



dotnet ef migrations remove 20181201024759_SmsMessageHistory -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create 

dotnet ef database update -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create



dotnet ef database update 20190127043623_ClientBodyLogging -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create 

REM rollback the propior migration
dotnet ef database update 20190307190857_SessionErrorUpdate -c UserDataContext  --startup-project ..\Whetstone.StoryEngine.Data.Create 

dotnet ef migrations remove -c UserDataContext --startup-project ..\Whetstone.StoryEngine.Data.Create 


REM Output full migration script
dotnet ef migrations script -i -o initcreate.sql


dotnet ef migrations add GuestUserUpdate -c UserDataContext  --startup-project ..\Whetstone.StoryEngine.Data.Create 


dotnet ef migrations add ConsentReportUpdate -c UserDataContext  --startup-project ..\Whetstone.StoryEngine.Data.Create 




dotnet ef migrations script -i -o msgexport.sql 20190811232825_InitialCreate MessageReportExport01

dotnet ef migrations script -i -o guestexport.sql GuestUserUpdate GuestUserUpdate

dotnet ef migrations script -i -o msgkeyupdate.sql MsgBackKeyUpdate MsgBackKeyUpdate

dotnet ef migrations script -i -c UserDataContext -o msgkeyupdate.sql 20191009182836_MsgBackKeyUpdate


dotnet ef migrations add Security01Update -c UserDataContext  --startup-project ..\Whetstone.StoryEngine.Data.Create 
dotnet ef migrations script -i -o securityupdate01.sql 20200611183241_Security01Update

dotnet ef migrations script 20200304143902_ConsentReportUpdate -o securityupdate01.sql


REM This creates a rollback script for the security update.
dotnet ef migrations script 20200611183241_Security01Update 20200304143902_ConsentReportUpdate -o rollbacksec.sql



dotnet ef migrations add Twitter01Update -c UserDataContext  --startup-project ..\Whetstone.StoryEngine.Data.Create 