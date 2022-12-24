using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Reporting.Models;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public class ReportDataRetrieverFunction : IReportDataRetriever
    {

        private ILogger<ReportDataRetrieverFunction> _logger; // = StoryEngineLogFactory.CreateLogger<ReportDataRetrieverFunction>();

        private IUserContextRetriever _userContextRetriever;

        public ReportDataRetrieverFunction(IUserContextRetriever contextRetriever, ILogger<ReportDataRetrieverFunction> logger)
        {

            _userContextRetriever = contextRetriever ??
                                    throw new ArgumentNullException($"{nameof(contextRetriever)}");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }


        public async Task<DataSet> GetReportDataAsync(ReportDataSourceBase reportDataSource,
            Dictionary<string, dynamic> parameters)
        {
            if (reportDataSource == null)
                throw new ArgumentNullException($"{nameof(reportDataSource)}");


            DataSet retSet = null;

            ReportSourceDatabaseFunction functionDef = reportDataSource as ReportSourceDatabaseFunction;

            if (functionDef == null)
                throw new ArgumentException($"{nameof(reportDataSource)} must be a ReportSourceDatabaseFunction type");

            string functionName = functionDef.FunctionName;

            if (string.IsNullOrWhiteSpace(functionName))
                throw new Exception($"Function name is missing from report definition");

            NpgsqlCommand pgcom = new NpgsqlCommand(functionName)
            {
                CommandType = CommandType.StoredProcedure
            };

            StringBuilder paramText = new StringBuilder();
            if (parameters != null)
            {
                if (functionDef.Parameters == null)
                    throw new Exception(
                        $"Parameter mismatch. {parameters.Count} parameters passed and no parameters defined in report definition.");

                StringBuilder mismatchBuilder = new StringBuilder();
                // apply parameters to the function
                foreach (string key in parameters.Keys)
                {
                    dynamic paramVal = parameters[key];

                    // find the parameter setting in the report definition

                    var foundParam = functionDef.Parameters.FirstOrDefault(x =>
                        x.ParameterName.Equals(key, StringComparison.OrdinalIgnoreCase));

                    if (foundParam == null)
                    {
                        mismatchBuilder.AppendLine($"Parameter {key} not found in report definition");
                    }
                    else
                    {
                        try
                        {
                            switch (foundParam.ParameterType)
                            {
                                case NpgsqlDbType.Uuid:

                                    if (paramVal is Guid)
                                    {
                                        pgcom.Parameters.AddWithValue(foundParam.ParameterName, NpgsqlDbType.Uuid,
                                            paramVal);
                                    }
                                    else
                                    {
                                        Guid parsedGuid = Guid.Parse((string)paramVal);
                                        pgcom.Parameters.AddWithValue(foundParam.ParameterName, NpgsqlDbType.Uuid,
                                            parsedGuid);
                                    }

                                    break;
                                case NpgsqlDbType.Timestamp:
                                    DateTime parsedDateTime;
                                    if (paramVal is string val)
                                    {
                                        parsedDateTime = DateTime.Parse(val);
                                    }
                                    else if (paramVal is DateTime time)
                                    {
                                        parsedDateTime = time;
                                    }
                                    else
                                    {
                                        parsedDateTime = (DateTime)paramVal;

                                    }

                                    pgcom.Parameters.AddWithValue(foundParam.ParameterName, NpgsqlDbType.Timestamp,
                                        parsedDateTime);
                                    break;
                                case NpgsqlDbType.Boolean:
                                    Boolean parsedBool = Boolean.Parse((string)paramVal);
                                    pgcom.Parameters.AddWithValue(foundParam.ParameterName, NpgsqlDbType.Boolean,
                                        parsedBool);
                                    break;
                                case NpgsqlDbType.Integer:
                                    int parsedInt = int.Parse((string)paramVal);
                                    pgcom.Parameters.AddWithValue(foundParam.ParameterName, NpgsqlDbType.Integer,
                                        parsedInt);
                                    break;

                                case NpgsqlDbType.Text:
                                    pgcom.Parameters.AddWithValue(foundParam.ParameterName, NpgsqlDbType.Text,
                                        (string)paramVal);
                                    break;
                                default:
                                    pgcom.Parameters.AddWithValue(foundParam.ParameterName, foundParam.ParameterType,
                                        paramVal);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {

                            throw new Exception(
                                $"Error setting parameter value for {foundParam.ParameterName} for type {foundParam.ParameterType}", ex);
                        }



                        paramText.AppendLine($"{foundParam.ParameterName}: {paramVal}");
                    }

                }

                // If there are any errors aggregated while mapping parameters, then
                // roll them up and throw an exception here.
                string mismatchInfo = mismatchBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(mismatchInfo))
                    throw new Exception(mismatchInfo);

            }



            string conStr = await _userContextRetriever.GetConnectionStringAsync();


            using (NpgsqlConnection pgcon = new NpgsqlConnection(conStr))
            {

                pgcom.Connection = pgcon;
                Stopwatch dbTime = new Stopwatch();
                try
                {
                    dbTime.Start();
                    await pgcon.OpenAsync();

                    NpgsqlDataAdapter pgDataAdapter = new NpgsqlDataAdapter(pgcom);
                    retSet = new DataSet();
                    pgDataAdapter.Fill(retSet);
                }
                catch (Exception ex)
                {
                    StringBuilder errBuilder = new StringBuilder();
                    errBuilder.AppendLine($"Error invoking PostgreSQL function {functionName} with parameters: ");
                    errBuilder.AppendLine(paramText.ToString());
                    _logger.LogError(ex, errBuilder.ToString());
                    throw;
                }
                finally
                {
                    dbTime.Stop();
                    pgcon.Close();
                    _logger.LogInformation($"Time to invoke reporting function {functionName}: {dbTime.ElapsedMilliseconds}");
                }

            }

            return retSet;
        }
    }
}
