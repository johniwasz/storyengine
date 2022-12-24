using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Reporting.Models;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public interface IReportDataRetriever
    {

        Task<DataSet> GetReportDataAsync(ReportDataSourceBase reportDataSource, Dictionary<string, dynamic> parameters);


    }
}
