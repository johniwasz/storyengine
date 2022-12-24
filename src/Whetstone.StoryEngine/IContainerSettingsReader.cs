using Amazon;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine
{


    /// <summary>
    /// Uses the BOOTSTRAP setting from the environment variables to load a YAML formatted
    /// bootstrapping configuration file.
    /// </summary>
    /// <returns></returns>
    public interface IContainerSettingsReader
    {

        string BootstrapParameter { get; }


        LogLevel LogLevel { get; }


        RegionEndpoint GetAwsEndpoint();

    }
}
