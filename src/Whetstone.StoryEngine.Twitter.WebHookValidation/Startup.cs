using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Tweetinvi.Models;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.WebLibrary;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.EntityFramework;

namespace Whetstone.StoryEngine.Twitter.WebHookValidation
{
    public class Startup
    {
        public const string AppS3BucketKey = "AppS3Bucket";


        public Startup(IConfiguration configuration)
        {
            Configuration = Bootstrapping.BuildConfiguration();
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {

            BootstrapConfig bootstrapConfig = Configuration.Get<BootstrapConfig>();

            services.UseStoryEngineServices(Configuration, bootstrapConfig);

            services.AddTransient<ITwitterValidator, TwitterValidator>();

            services.AddControllers()
            .AddNewtonsoftJson(options =>
                  {

                      options.SerializerSettings.Converters.Add(new StringEnumConverter()
                      {
                          NamingStrategy = new CamelCaseNamingStrategy()
                      });

                      options.SerializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
                      //options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                      options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                      options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                      options.SerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Auto;
                      options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                  });



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
         //   app.UseTweetinviWebhooks(config);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseXRay("WhetstoneTwitterApi");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
