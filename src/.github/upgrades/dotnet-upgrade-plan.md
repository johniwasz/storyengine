# .NET 9.0 Upgrade Plan

## Execution Steps

1. Validate that an .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade projects to .NET 9.0.
  - 3.1. Upgrade Whetstone.StoryEngine.Cache.csproj
  - 3.2. Upgrade Whetstone.StoryEngine.Models.csproj
  - 3.3. Upgrade Whetstone.StoryEngine.csproj
  - 3.4. Upgrade Whetstone.StoryEngine.Data.csproj
  - 3.5. Upgrade Whetstone.StoryEngine.Repository.csproj
  - 3.6. Upgrade Whetstone.StoryEngine.Cache.DynamoDB.csproj
  - 3.7. Upgrade Whetstone.StoryEngine.DependencyInjection.csproj
  - 3.8. Upgrade Whetstone.StoryEngine.Security.csproj
  - 3.9. Upgrade Whetstone.StoryEngine.Data.EntityFramework.csproj
  - 3.10. Upgrade Whetstone.StoryEngine.Data.DependencyInjection.csproj
  - 3.11. Upgrade Whetstone.StoryEngine.Reporting.Models.csproj
  - 3.12. Upgrade Whetstone.StoryEngine.InboundSmsRepository.csproj
  - 3.13. Upgrade Whetstone.StoryEngine.Google.Management.csproj
  - 3.14. Upgrade Whetstone.StoryEngine.Reporting.ReportRepository.csproj
  - 3.15. Upgrade Whetstone.StoryEngine.AlexaProcessor.csproj
  - 3.16. Upgrade Whetstone.Google.Actions.csproj
  - 3.17. Upgrade Whetstone.StoryEngine.SocketApi.Repository.csproj
  - 3.18. Upgrade Whetstone.StoryEngine.InboundSmsHandler.csproj
  - 3.19. Upgrade Whetstone.StoryEngine.MessageSender.csproj
  - 3.20. Upgrade Whetstone.StoryEngine.WebLibrary.csproj
  - 3.21. Upgrade Whetstone.StoryEngine.MessageSender.SaveMessageTask.csproj
  - 3.22. Upgrade Whetstone.StoryEngine.Bixby.Repository.csproj
  - 3.23. Upgrade Whetstone.StoryEngine.Google.Repository.csproj
  - 3.24. Upgrade Whetstone.Queue.SessionLogger.Repository.csproj
  - 3.25. Upgrade Whetstone.StoryEngine.Notifications.Repository.csproj
  - 3.26. Upgrade Whetstone.StoryEngine.Test.csproj
  - 3.27. Upgrade Whetstone.Queue.TwiliosStatusUpdate.csproj
  - 3.28. Upgrade Whetstone.StoryEngine.AlexaFunction.csproj
  - 3.29. Upgrade Whetstone.StoryEngine.LambdaUtilities.csproj
  - 3.30. Upgrade Whetstone.StoryEngine.ConfigUtilities.csproj
  - 3.31. Upgrade Whetstone.StoryEngine.AudioProcessor.Repository.csproj
  - 3.32. Upgrade Whetstone.StoryEngine.Bixby.LambdaHost.csproj
  - 3.33. Upgrade Whetstone.StoryEngine.Google.Actions.LambdaHost.csproj
  - 3.34. Upgrade Whetstone.StoryEngine.Google.LambdaHost.csproj
  - 3.35. Upgrade Whetstone.StoryEngine.Reporting.ReportGenerator.csproj
  - 3.36. Upgrade Whetstone.Queue.SessionLogger.csproj
  - 3.37. Upgrade Whetstone.StoryEngine.CoreApi.csproj
  - 3.38. Upgrade Whetstone.StoryEngine.AlexaFunction.Test.csproj
  - 3.39. Upgrade Whetstone.StoryEngine.LambdaUtilities.Tests.csproj
  - 3.40. Upgrade Whetstone.StoryEngine.ConfigUtilities.Tests.csproj
  - 3.41. Upgrade Whetstone.StoryEngine.AudioProcessor.Lambda.csproj
  - 3.42. Upgrade Whetstone.StoryEngine.Notifications.Lambda.csproj
  - 3.43. Upgrade Whetstone.StoryEngine.SocketApi.Auth.csproj
  - 3.44. Upgrade Whetstone.StoryEngine.SocketApi.csproj
  - 3.45. Upgrade Whetstone.StoryEngine.Cache.DynamoDB.Test.csproj
  - 3.46. Upgrade Whetstone.StoryEngine.Bixby.WebApiHost.csproj
  - 3.47. Upgrade Whetstone.StoryEngine.SmsCallback.csproj
  - 3.48. Upgrade Whetstone.StoryEngine.Google.WebApiHost.csproj
  - 3.49. Upgrade Whetstone.StoryEngine.Reporting.ReportGenerator.Tests.csproj
  - 3.50. Upgrade Whetstone.StoryEngine.DynamoDBMonitors.csproj
  - 3.51. Upgrade Whetstone.StoryEngine.UnitTests.csproj
  - 3.52. Upgrade Whetstone.StoryEngine.Data.Tests.csproj
  - 3.53. Upgrade Whetstone.Queue.SessionLogger.Tests.csproj
  - 3.54. Upgrade Whetstone.StoryEngine.CoreApi.Tests.csproj
  - 3.55. Upgrade Whetstone.StoryEngine.Data.Create.csproj
4. Run unit tests to validate upgrade in the projects listed below:
  - Whetstone.StoryEngine.Test.csproj
  - Whetstone.StoryEngine.CoreApi.Tests.csproj
  - Whetstone.Queue.SessionLogger.Tests.csproj
  - Whetstone.StoryEngine.Data.Tests.csproj
  - Whetstone.StoryEngine.UnitTests.csproj
  - Whetstone.StoryEngine.Reporting.ReportGenerator.Tests.csproj
  - Whetstone.StoryEngine.LambdaUtilities.Tests.csproj
  - Whetstone.StoryEngine.ConfigUtilities.Tests.csproj
  - Whetstone.StoryEngine.Cache.DynamoDB.Test.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

| Package Name                        | Current Version | New Version | Description                         |
|:------------------------------------|:---------------:|:-----------:|:------------------------------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.AspNetCore.Authorization | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.AspNetCore.TestHost | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.EntityFrameworkCore | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.EntityFrameworkCore.Design | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.EntityFrameworkCore.Relational | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Caching.Abstractions | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Caching.Memory | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Configuration | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Configuration.Binder | 7.0.1 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Configuration.EnvironmentVariables | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.DependencyInjection | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.DependencyInjection.Abstractions | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Logging | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Logging.Abstractions | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Logging.Console | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Logging.Debug | 7.0.0 | 9.0.5 | Latest stable version |
| Microsoft.Extensions.Options | 7.0.0 | 9.0.5 | Latest stable version |
| MessagePack | 2.4.59 | 3.1.3 | Security vulnerability, latest stable |
| Npgsql | 7.0.1 | 9.0.3 | Security vulnerability, latest stable |
| Newtonsoft.Json | 13.0.2 | 13.0.3 | Latest stable version |
| System.Configuration.ConfigurationManager | 7.0.0 | 9.0.5 | Latest stable version |
| EntityFramework | 6.4.4 | 6.5.1 | Latest stable version |
| Whetstone.Alexa | 0.1.6 | 0.1.6 | Deprecated, no newer stable |
| Microsoft.Extensions.Caching.Redis | 2.2.0 | 2.3.0 | Deprecated, latest stable |
| Microsoft.AspNetCore.Http | 2.2.2 | 2.3.0 | Deprecated, latest stable |
| Microsoft.AspNetCore.Http.Abstractions | 2.2.0 | 2.3.0 | Deprecated, latest stable |
| Microsoft.AspNetCore.Http.Extensions | 2.2.0 | 2.3.0 | Deprecated, latest stable |
| Microsoft.AspNetCore.Mvc.Formatters.Json | 2.2.0 | 2.3.0 | Deprecated, latest stable |
| Microsoft.AspNetCore.Mvc.Formatters.Xml | 2.2.0 | 2.3.0 | Deprecated, latest stable |
| Microsoft.AspNetCore.Mvc.Core | 2.2.5 | 2.3.0 | Deprecated, latest stable |
| Microsoft.EntityFrameworkCore.Relational.Design | 1.1.6 | 1.1.6 | Deprecated, no newer stable |
| Npgsql.EntityFrameworkCore.PostgreSQL.Design | 1.1.0 | 1.1.0 | Deprecated, no newer stable |

### Project upgrade details

... (project details updated to reflect all latest stable package upgrades and removal of included-in-framework packages)
