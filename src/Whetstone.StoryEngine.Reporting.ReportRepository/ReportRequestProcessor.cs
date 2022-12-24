using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Reporting.Models;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public class ReportRequestProcessor : IReportRequestProcessor
    {

        private IReportDefinitionRetriever _reportDefRetriever = null;

        private Func<ReportDataSourceType, IReportDataRetriever> _dataSourceFunc = null;

        private Func<ReportOutputType, IReportFormatter> _outputFormatterFunc = null;

        public ReportRequestProcessor(IReportDefinitionRetriever reportRetriever,
            Func<ReportDataSourceType, IReportDataRetriever> dataSourceFunc,
            Func<ReportOutputType, IReportFormatter> outputFormatterFunc)
        {

            _reportDefRetriever = reportRetriever ??
                                  throw new ArgumentNullException($"{nameof(reportRetriever)}");

            _dataSourceFunc = dataSourceFunc ??
                              throw new ArgumentNullException($"{nameof(dataSourceFunc)}");

            _outputFormatterFunc = outputFormatterFunc ??
                            throw new ArgumentNullException($"{nameof(outputFormatterFunc)}");


        }


        public async Task<ReportSendStatus> ProcessReportRequestAsync(ReportRequest repRequest)
        {

            if (repRequest == null)
                throw new ArgumentNullException($"{nameof(repRequest)}l");

            if (string.IsNullOrWhiteSpace(repRequest.ReportName))
                throw new ArgumentException($"{nameof(repRequest)} ReportName property cannot be null or empty");


            ReportDefinition reportDef = await _reportDefRetriever.GetReportDefinitionAsync(repRequest.ReportName);

            if (reportDef == null)
                throw new Exception($"Report definition {repRequest.ReportName} not found");


            if (reportDef.DataSource == null)
                throw new Exception(
                    $"Invalid report definition configuration. Report definition {repRequest.ReportName} does not include a data source");

            if (reportDef.Output == null)
                throw new Exception(
                    $"Invalid report definition configuration. Report definition {repRequest.ReportName} does not include an output format");


            IReportDataRetriever dataRetriever = _dataSourceFunc(reportDef.DataSource.DataSourceType);


            DataSet returnSet = await dataRetriever.GetReportDataAsync(reportDef.DataSource, repRequest.Parameters);

            if (returnSet == null)
                throw new Exception($"No dataset returned for report request {repRequest.ReportName}");

            // Generate export file.
            IReportFormatter reportFormatter = _outputFormatterFunc(reportDef.Output.OutputType);


            List<FileSendStatus> outputPaths = await reportFormatter.OutputReportAsync(repRequest.ReportName,
                returnSet,
                reportDef.Output,
                reportDef.FileNameFormat,
                repRequest);

            // Get the destination definition.

            ReportSendStatus repOutput = new ReportSendStatus
            {
                OutputFiles = outputPaths,
                Destination = reportDef.Destination
            };


            return repOutput;

        }
    }
}
