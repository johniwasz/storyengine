using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Whetstone.StoryEngine.DependencyInjection;

namespace Whetstone.StoryEngine.Twitter.WebHookValidation
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
            {
                // Uncomment these for dev.
                Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
                Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "AdminUser");
                Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);

                // Uncomment these for prod.
                // Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/Prod/bootstrap");
                // Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "AdminUser");
                // Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USWest2.SystemName);


            }



            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
