using System;
using System.Collections.Generic;
using NpgsqlTypes;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{


    /// <summary>
    /// This class defines the structure of the report and where to retrieve the data.
    /// </summary>
    public class ReportDefinition
    {
        [YamlMember(Alias = "dataSource")]
        public ReportDataSourceBase DataSource { get; set; }

        [YamlMember(Alias = "output")]
        public ReportOutputBase Output { get; set; }

        [YamlMember(Alias = "destination")]
        public ReportDestinationBase Destination { get; set; }

        [YamlMember(Alias = "fileNameFormat")]
        public string FileNameFormat { get; set; }

    }














}
