using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Reporting.Models;


namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public class ReportFormatterCsv : IReportFormatter
    {

        private readonly ILogger<ReportFormatterCsv> _logger = null;
        private IFileRepository _fileRep = null;

        public ReportFormatterCsv(IFileRepository fileRep, ILogger<ReportFormatterCsv> logger)
        {
            _fileRep = fileRep ?? throw new ArgumentNullException($"{nameof(fileRep)}");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }


        /// <summary>
        /// Returns the S3 key to the generated report.
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="reportData"></param>
        /// <param name="formatDefinition"></param>
        /// <param name="outputFileName"></param>
        /// <returns></returns>
        public async Task<List<FileSendStatus>> OutputReportAsync(string reportName, DataSet reportData, ReportOutputBase formatDefinition, string outputFileName, ReportRequest request)
        {

            if (reportData == null)
                throw new ArgumentNullException($"{nameof(reportData)}");

            if (formatDefinition == null)
                throw new ArgumentNullException($"{nameof(formatDefinition)}");

            ReportOutputCsv csvDefinition = formatDefinition as ReportOutputCsv;

            if (csvDefinition == null)
                throw new Exception($"{nameof(formatDefinition)} must be type ReportOutputCsv");


            if (csvDefinition.Columns == null)
                throw new ArgumentException($"{nameof(csvDefinition)} Columns property cannot be null");

            if (csvDefinition.Columns.Count == 0)
                throw new ArgumentException($"{nameof(csvDefinition)} Columns property contains no columns");

            if (request == null)
                throw new ArgumentNullException($"{nameof(request)} ");


            DataTable outTable = null;
            // exactly one table expected in DataSet
            if (reportData.Tables.Count != 1)
                throw new Exception($"Only only table expected in {nameof(reportData)}. Found {reportData.Tables.Count}");

            outTable = reportData.Tables[0];

            StringBuilder colErrors = new StringBuilder();

            List<string> columns = new List<string>();
            // create the headers
            for (int i = 0; i < csvDefinition.Columns.Count; i++)
            {
                CsvColumnDefinition col = csvDefinition.Columns[i];
                if (col == null)
                {
                    colErrors.AppendLine($"Column in index {i} cannot be null");
                }
                else
                {
                    string colName = col.Name;
                    if (string.IsNullOrWhiteSpace(colName))
                        colErrors.AppendLine($"Column in index {i} name is null");
                    else
                        columns.Add(colName);
                }
            }

            if (columns.Count != outTable.Columns.Count)
                colErrors.AppendLine(
                    $"Mismatch in report column output. Report format column count: {columns.Count}. Columns in retrieved data: {outTable.Columns.Count}");


            string colErrText = colErrors.ToString();

            if (!string.IsNullOrWhiteSpace(colErrText))
                throw new Exception(colErrText);



            StringBuilder csvBuilder = new StringBuilder();
            csvBuilder.AppendLine(string.Join(",", columns));

            foreach (DataRow row in outTable.Rows)
            {

                List<string> rowValues = new List<string>();

                for (int i = 0; i < csvDefinition.Columns.Count; i++)
                {
                    //var colType = row.Table.Columns[i];
                    //colType.
                    CsvColumnDefinition colDef = csvDefinition.Columns[i];

                    string textVal = row[i] == null ? string.Empty :
                                string.IsNullOrWhiteSpace(colDef.Format) ?
                                    row[i].ToString() :
                                    string.Format(colDef.Format, row[i]);

                    if (!string.IsNullOrWhiteSpace(colDef.RegularExpressionFilter?.RegularExpression))
                    {
                        string regex = colDef.RegularExpressionFilter.RegularExpression;

                        string replaceText = string.IsNullOrWhiteSpace(colDef.RegularExpressionFilter.ReplacementText)
                            ? string.Empty
                            : colDef.RegularExpressionFilter.ReplacementText;

                        textVal = Regex.Replace(textVal, regex, replaceText);

                    }

                    if (colDef.Casing.HasValue)
                    {
                        switch (colDef.Casing.Value)
                        {
                            case ValueCasing.Lower:
                                textVal = textVal.ToLower();
                                break;
                            case ValueCasing.Upper:
                                textVal = textVal.ToUpper();
                                break;
                        }
                    }

                    bool isQuoted = colDef.IsQuoted.GetValueOrDefault(false);
                    if (isQuoted)
                        textVal = $"\"{textVal}\"";

                    rowValues.Add(textVal);
                }

                csvBuilder.AppendLine(string.Join(",", rowValues));
            }

            string csvContents = csvBuilder.ToString();

            string generatedFileName = string.IsNullOrWhiteSpace(outputFileName)
                ? Guid.NewGuid().ToString()
                : await MacroProcessing.ProcessMacrosAsync(outputFileName);


            string metadataOutFile = await SaveReportMetadataAsync(outTable.Rows.Count, request, generatedFileName);


            generatedFileName = $"{generatedFileName}.csv";

            string generatedMetaDataFile = $"{generatedFileName}.json";


            // Save the generated file
            string fullPath = $"reports/{reportName}/{generatedFileName}";


            await _fileRep.SetTextContentAsync(fullPath, csvContents, "text/csv");

            FileSendStatus sendStatus = new FileSendStatus
            {
                FilePath = fullPath,
                IsSent = false
            };

            List<FileSendStatus> outputFiles = new List<FileSendStatus>
            {
                sendStatus
            };


            return outputFiles;
        }

        private async Task<string> SaveReportMetadataAsync(int rowCount, ReportRequest repRequest, string outFile)
        {

            string outFileName = $"{outFile}.json";

            ReportMetadata repMetadata = new ReportMetadata
            {
                DateGenerated = DateTime.UtcNow,
                RowCount = rowCount,
                Parameters = repRequest.Parameters,
                DeliveryTime = repRequest.DeliveryTime.GetValueOrDefault(DateTime.UtcNow)
            };

            string reportMetaData = JsonConvert.SerializeObject(repMetadata, Formatting.Indented);

            string fullPath = $"reports/{repRequest.ReportName}/{outFileName}";

            await _fileRep.SetTextContentAsync(fullPath, reportMetaData, "application/json");


            return fullPath;
        }
    }
}
