using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.AlexaProcessor.Configuration;
using Whetstone.StoryEngine.DependencyInjection;
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
