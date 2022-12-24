using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{

    public class ReportSourceDatabaseFunction : ReportDataSourceBase
    {
        [YamlIgnore]
        public override ReportDataSourceType DataSourceType
        {
            get => ReportDataSourceType.DatabaseFunction;
            set
            {
                // do nothing
            }
        }


        /// <summary>
        /// Fully qualified name of the database function to call.
        /// </summary>
        [YamlMember(Alias = "functionName")]
        public string FunctionName { get; set; }



        [YamlMember(Alias = "parameters")]
        public List<FunctionParameter> Parameters { get; set; }



    }
}
