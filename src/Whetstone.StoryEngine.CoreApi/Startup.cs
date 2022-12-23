
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.WebLibrary;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Whetstone.StoryEngine.CoreApi.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using System.Net.Mime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Security.Amazon;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Security;
using Whetstone.StoryEngine.SocketApi.Repository.Extensions;
using Whetstone.StoryEngine.CoreApi.WebSockets;
#if !DEBUG
using Whetstone.StoryEngine.Notifications.Repository.Extensions;
#endif

namespace Whetstone.StoryEngine.CoreApi
{
    public class StartUp 
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public StartUp(IConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
        {
//            Logger.LogDebug("Entering CoreApi BuildConfiguration");
            Configuration = Bootstrapping.BuildConfiguration();
        }


        public static IConfiguration Configuration { get; private set; }



        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //  Logger.LogDebug("Entering CoreApi ConfigureServices");

            BootstrapConfig bootstrapConfig = Configuration.Get<BootstrapConfig>();

            services.UseStoryEngineServices(Configuration, bootstrapConfig);

            services.AddTransient<ProblemDetailsFactory, EngineProblemFactory>();

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var result = new BadRequestObjectResult(context.ModelState);

                        result.ContentTypes.Add(MediaTypeNames.Application.Json);
                        result.ContentTypes.Add(MediaTypeNames.Application.Xml);

                        return result;
                    };
                });



            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new YamlOutputFormatter());
                options.InputFormatters.Add(new YamlInputFormatter());
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());

                options.RespectBrowserAcceptHeader = true;
                options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "text/xml");
                options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
                options.FormatterMappings.SetMediaTypeMappingForFormat("yaml", "application/yaml");
                options.FormatterMappings.SetMediaTypeMappingForFormat("yml", "application/yaml");
                //options.FormatterMappings.SetMediaTypeMappingForFormat("js", "application/json");
                //options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");

            })
            .AddXmlSerializerFormatters()
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

            
            if (bootstrapConfig.Security.AuthenticatorType.GetValueOrDefault(AuthenticatorType.Cognito) ==
                AuthenticatorType.Cognito)
            {

                CognitoConfig cogConfig = bootstrapConfig.Security.Cognito;

               // Setup authentication
               services
                   .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, "bearer",options =>
                   {
                        options.SaveToken = true;
                        options.Audience = cogConfig.UserPoolClientId;
                        options.TokenValidationParameters = 
                            CognitoAuthenticator.GetTokenValidationParameters(cogConfig);

                        options.Authority = options.TokenValidationParameters.ValidIssuer;
                        options.Events = new JwtBearerEvents()
                        {
                            OnChallenge = async ctx =>
                            {

                                //ctx.HttpContext.Items.Add("authfailure", ctx.Exception);

                                if (ctx.AuthenticateFailure != null)
                                {
                                    ctx.Response.StatusCode = 401;
                                    ctx.Response.ContentType = "application/json";

                                    Exception curEx = ctx.AuthenticateFailure is SecurityTokenExpiredException
                                        ? new TokenIsExpiredException("token expired", "token expired", ctx.AuthenticateFailure)
                                        : ctx.AuthenticateFailure;


                                    ProblemDetails probDetails = new ProblemDetails();
                                    if (curEx is AdminException adminEx)
                                    {
                                        probDetails.Title = adminEx.Title;

                                        probDetails.Detail = adminEx.PublicMessage;
                                        probDetails.Extensions.Add("errorCode", adminEx.ErrorCode);
                                    }
                                    else
                                    {
                                        probDetails.Title = "Not Authenticated";
                                    }

                                    JsonSerializerSettings serSettings = new JsonSerializerSettings();

                                    serSettings.Converters.Add(new StringEnumConverter()
                                    {
                                        NamingStrategy = new CamelCaseNamingStrategy()
                                    });

                                    serSettings.ConstructorHandling =
                                        ConstructorHandling.AllowNonPublicDefaultConstructor;
                                    //options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                                    serSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                                    serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                    serSettings.ObjectCreationHandling = ObjectCreationHandling.Auto;
                                    serSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

                                    await ctx.Response.WriteAsync(
                                        JsonConvert.SerializeObject(probDetails, serSettings));

                                }

                                ctx.HandleResponse();

                            },



                            OnTokenValidated = async ctx =>
                            {
                                //Get EF context
                                var authenticator = ctx.HttpContext.RequestServices.GetService<IAuthenticator>();

                                //Check is user a super admin
                                var claimIdentities =
                                    await authenticator.GetUserClaimsAsync(ctx.SecurityToken as JwtSecurityToken);

                                ctx.Principal.AddIdentities(claimIdentities);
                            }
                        };
                   });

            }

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());

            });

            // Add Socket Options to centralize how this is setup
            services.ConfigureSocketOptions(bootstrapConfig);

#if DEBUG
            //
            // Put WebSocket infrastructure in place when we are running debug so
            // we can test websocket calls from the client without going through API Gateway
            //
            services.EnableDebugWebSockets( bootstrapConfig );

            // Add an in-proc notification processor
            services.EnableDebugNotificationProcessor(bootstrapConfig);
#else
            // Add a lambda based notification processor
            services.AddNotificationProcessor(bootstrapConfig);
#endif
        }

     

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseErrorLogging();
            app.UseExceptionHandler("/error");

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        NoCache = true,
                        NoStore = true,
                        MustRevalidate = true,
                    };
                context.Response.Headers.Add("Expires", "0");
                context.Response.Headers.Add("Pragma", "no-cache");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                if (context.Items.ContainsKey("authfailure"))
                {
                    if (context.Items["authfailure"] is Exception ex)
                        throw ex;
                }
                else
                {
                    await next();
                }

            });




            app.UseXRay("WhetstoneCoreApi");
#if DEBUG
            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            //
            // Put WebSocket infrastructure in place when we are running debug so
            // we can test websocket calls from the client without going through API Gateway
            //
            app.EnableDebugWebSockets();

#else
            app.UseCors("CorsPolicy");
#endif

            


            //app.UseSwagger();

            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Story Engine API");
            //});

           



            app.UseRouting();

           

            app.UseAuthentication();
            app.UseAuthorization();


            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            

        }
    }
}
