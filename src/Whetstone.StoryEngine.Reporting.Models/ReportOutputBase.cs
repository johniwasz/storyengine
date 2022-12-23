using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Reporting.Models
{


    public enum ReportOutputType
    {
        Csv = 1,
        Json = 2
    }

    public abstract class ReportOutputBase
    {

        public abstract ReportOutputType OutputType { get; set; }

    }
}
