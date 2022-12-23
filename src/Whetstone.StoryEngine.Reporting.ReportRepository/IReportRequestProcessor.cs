using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Reporting.Models;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public interface IReportRequestProcessor
    {



        Task<ReportSendStatus> ProcessReportRequestAsync(ReportRequest repRequest);

    }
}
