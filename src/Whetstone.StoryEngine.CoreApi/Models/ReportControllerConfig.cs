using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class ReportControllerConfig
    {

        /// <summary>
        /// ARN of the reporting step function.
        /// </summary>
        public string ReportStepFunction { get; set; }

    }
}
