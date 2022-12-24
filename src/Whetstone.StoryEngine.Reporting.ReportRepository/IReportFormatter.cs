using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Reporting.Models;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public interface IReportFormatter
    {


        Task<List<FileSendStatus>> OutputReportAsync(string reportName, DataSet reportData, ReportOutputBase formatDefinition, string outputFileName, ReportRequest request);



    }
}
