using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Reporting.Models.Serialization;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public class ReportDefinitionRetrieverS3 : IReportDefinitionRetriever
    {
        private const string REPORTFOLDER = "definitions";

        private ILogger<ReportDefinitionRetrieverS3> _logger;
        private IFileRepository _fileRep = null;

        public ReportDefinitionRetrieverS3(IFileRepository fileRep, ILogger<ReportDefinitionRetrieverS3> logger)
        {
            _fileRep = fileRep ?? throw new ArgumentNullException($"{nameof(fileRep)}");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }

        public async Task<ReportDefinition> GetReportDefinitionAsync(string reportName)
        {

            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentNullException($"{nameof(reportName)}");

            ReportDefinition repDef = null;
            try
            {
                string reportDefText;
                string reportPath = $"{REPORTFOLDER}/{reportName}.yml";

                var repDeser = YamlReportSerializer.GetYamlDeserializer();
                Stopwatch reportDefRetrievalTime = Stopwatch.StartNew();
                reportDefText = await _fileRep.GetTextContentAsync(reportPath);
                reportDefRetrievalTime.Stop();
                _logger.LogInformation($"Time to retrieve report definition {reportName}: {reportDefRetrievalTime.ElapsedMilliseconds}ms");

                repDef = repDeser.Deserialize<ReportDefinition>(reportDefText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error getting report definition {reportName}");
                throw;
            }

            return repDef;
        }
    }
}
