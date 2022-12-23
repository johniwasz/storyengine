using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{
    public class ReportOutputCsv : ReportOutputBase
    {
        [YamlIgnore]
        public override ReportOutputType OutputType
        {
            get => ReportOutputType.Csv;
            set
            {
                // do nothing
            }
        }

        [YamlMember(Alias = "columns")]
        public List<CsvColumnDefinition> Columns { get; set; }

    }

}
