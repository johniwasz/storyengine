using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Repository.Messaging;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class DataMessageReporter : IMessageReporter
    {
        private IUserContextRetriever _contextRetriever;



        private readonly ILogger<DataMessageReporter> _dataLogger;


        public DataMessageReporter(IUserContextRetriever contextRetriever, ILogger<DataMessageReporter> logger)
        {

            _contextRetriever = contextRetriever ?? throw new ArgumentNullException($"{nameof(contextRetriever)}");

            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }



        public async Task<List<MessageConsentReportRecord>> GetMessageConsentReportAsync(MessageConsentReportRequest reportRequest)
        {
            List<MessageConsentReportRecord> retList = null;

            if (reportRequest == null)
                throw new ArgumentNullException($"{nameof(reportRequest)}");

            if (reportRequest.TitleId == default(Guid))
                throw new ArgumentNullException($"{nameof(reportRequest)}", "TitleId property must have a valid value");

            if (reportRequest.StartTime == default(DateTime))
                throw new ArgumentNullException($"{nameof(reportRequest)}", "StartTime property must have a valid value");

            if (reportRequest.EndTime == default(DateTime))
                throw new ArgumentNullException($"{nameof(reportRequest)}", "EndTime property must have a valid value");


            if (reportRequest.StartTime > reportRequest.EndTime)
                throw new ArgumentException($"{nameof(reportRequest)} StartTime property value must come before the EndTime property value");


            try
            {

                using (var userContext = await _contextRetriever.GetUserDataContextAsync())
                {
                    retList = await userContext.GetMessageConsentReportAsync(reportRequest);
                }
            }
            catch (PostgresException postEx)
            {

                _dataLogger.LogError(postEx,
                    $"Unexpected PostgreSQL exception getting message report from database with titleid {reportRequest.TitleId} from {reportRequest.StartTime} to {reportRequest.EndTime}");
                throw;

            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex,
                    $"Unexpected exception getting message report from database with titleid {reportRequest.TitleId} from {reportRequest.StartTime} to {reportRequest.EndTime}");
                throw;

            }


            return retList;

        }
    }
}
