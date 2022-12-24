using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.DependencyInjection;
using Amazon.Util.Internal;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.AlexaProcessor.Configuration;
using Whetstone.StoryEngine.AlexaProcessor;
var hostBuilder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults();


hostBuilder.ConfigureStoryEngine();

hostBuilder.ConfigureServices(services =>
{
    services.Configure<AlexaConfig>(
        options =>
        {
            options.EnforceAlexaPolicyCheck = true;
            options.AuditClientMessages = false;
        });

    services.AddTransient<IAlexaRequestProcessor, AlexaRequestProcessor>();
});


var host = hostBuilder.Build();

host.Run();
