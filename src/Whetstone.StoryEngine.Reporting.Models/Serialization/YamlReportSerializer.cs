using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Whetstone.StoryEngine.Reporting.Models.Serialization
{


    public static class YamlReportSerializer
    {


        public static ISerializer GetYamlSerializer()
        {


            var yamlSerializer = new SerializerBuilder()
                .DisableAliases()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTagMapping("!ds-function", typeof(ReportSourceDatabaseFunction))
                .WithTagMapping("!ot-csv", typeof(ReportOutputCsv))
                .WithTagMapping("!rd-sftp", typeof(ReportDestinationSftp))
                .Build();

            return yamlSerializer;
        }


        public static IDeserializer GetYamlDeserializer()
        {

            var yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .WithTagMapping("!ds-function", typeof(ReportSourceDatabaseFunction))
                .WithTagMapping("!ot-csv", typeof(ReportOutputCsv))
                .WithTagMapping("!rd-sftp", typeof(ReportDestinationSftp))
                .Build();

            return yamlDeserializer;
        }
    }
}

