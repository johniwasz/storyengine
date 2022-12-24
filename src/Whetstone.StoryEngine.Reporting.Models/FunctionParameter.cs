using NpgsqlTypes;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{
    public class FunctionParameter
    {

        [YamlMember(Alias = "parameterName")]
        public string ParameterName { get; set; }

        [YamlMember(Alias = "parameterType")]
        public NpgsqlDbType ParameterType { get; set; }

    }
}
