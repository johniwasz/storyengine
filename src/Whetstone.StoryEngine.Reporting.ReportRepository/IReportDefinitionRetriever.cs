using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Reporting.Models;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public interface IReportDefinitionRetriever
    {

        Task<ReportDefinition> GetReportDefinitionAsync(string reportName);

    }
}
