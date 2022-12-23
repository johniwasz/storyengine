using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;
using Whetstone.StoryEngine.Data.EntityFramework;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.Data.Create
{
    [ExcludeFromCodeCoverage]
    public class UserDataContextFactory : IDesignTimeDbContextFactory<UserDataContext>
    {
        public UserDataContext CreateDbContext(string[] args)
        {

          //  string bootstrapYaml = GetKeyContent(@"/storyengine/dev/bootstrap");


       //     BootstrapConfig bootConfig = YamlD

          //  string connection = GetKeyContent($"/storyengine/dev/enginedb");

            var optionsBuilder = new DbContextOptionsBuilder<UserDataContext>();

            optionsBuilder.EnableSensitiveDataLogging();

       //     EnvironmentConfig envConfig = new EnvironmentConfig(RegionEndpoint.USEast1, "");


          //  IUserContextRetriever contextRetriever = new IamUserContextRetriever();

           string   connection = "Host=localhost;Database=postgres;Username=postgres;Password=postgres";

            optionsBuilder.UseNpgsql(connection, x=>
            {
                x.MigrationsHistoryTable("efmigrationhistory", "whetstone");
                x.MigrationsAssembly("Whetstone.StoryEngine.Data.Create");
            });

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();

#if DEBUG
                builder.AddDebug();
#endif

            });

            ILogger<UserDataContext> contextLogger = loggerFactory.CreateLogger<UserDataContext>();


            UserDataContext context = new UserDataContext(optionsBuilder.Options, contextLogger);

            return context;
        }

    }
}
