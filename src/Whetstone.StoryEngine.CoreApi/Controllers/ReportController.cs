using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Whetstone.StoryEngine.CoreApi.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Reporting.ReportRepository;
using Whetstone.StoryEngine.Repository.Messaging;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{

    //  [EnableCors("CorsPolicy")]
    [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
    [Route("api/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {

        private readonly ILogger _reportLogger;
        private readonly IMessageReporter _messageReporter;
        private readonly IReportRequestReceiver _reportRequestReceiver;


        public ReportController(IReportRequestReceiver reportRequestReceiver, IMessageReporter messageReporter, ILogger<ReportController> logger)
        {
            _reportRequestReceiver = reportRequestReceiver ??
                                     throw new ArgumentNullException($"{nameof(reportRequestReceiver)}");

            _messageReporter = messageReporter ??
                               throw new ArgumentNullException($"{nameof(messageReporter)}");

            _reportLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }

        [HttpPost("schedulereport", Name = "Post")]
        public async Task<IActionResult> ScheduleReportAsync([FromBody] ReportRequest repRequest)
        {
            if (repRequest == null)
                return BadRequest("body of request cannot be null");

            if (string.IsNullOrWhiteSpace(repRequest.ReportName))
                return BadRequest("reportName cannot be null or empty");


            if (repRequest.DeliveryTime.HasValue)
                repRequest.IsScheduled = true;
            string reportRequestJson;

            bool isError;
            // Post the report request to the step function.
            try
            {
                reportRequestJson = JsonConvert.SerializeObject(repRequest, Formatting.Indented);
                await _reportRequestReceiver.SaveReportRequestAsync(repRequest);
                isError = false;
            }
            catch (Exception ex)
            {
                isError = true;
                _reportLogger.LogError(ex,
                    string.IsNullOrWhiteSpace(null)
                        ? "Error processing report request"
                        : $"Error processing report request: {(string)null}");
            }

            if (isError)
                return new StatusCodeResult(500);


            return new AcceptedResult();
        }


        // GET: api/Report/5
        [HttpGet("message/{titleId}", Name = "Get")]
        public async Task<IActionResult> Get(string titleId, [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {

            StringBuilder errMsgBuilder = new StringBuilder();

            if (!Guid.TryParse(titleId, out Guid titleIdGuid))
            {
                errMsgBuilder.AppendLine($"{nameof(titleId)} is not a valid Guid.");
            }

            if (startTime == default)
            {
                errMsgBuilder.AppendLine($"{nameof(startTime)} is not a valid date");
            }

            if (endTime == default)
            {
                errMsgBuilder.AppendLine($"{nameof(endTime)} is not a valid date");
            }

            if (startTime > endTime)
            {
                errMsgBuilder.AppendLine($"{nameof(startTime)} must be prior to {nameof(endTime)}");
            }

            string errText = errMsgBuilder.ToString();


            if (!string.IsNullOrWhiteSpace(errText))
            {
                return BadRequest(errText);
            }

            MessageConsentReportRequest reportRequest = new MessageConsentReportRequest
            {
                TitleId = titleIdGuid,
                StartTime = startTime,
                EndTime = endTime
            };

            bool isError = false;
            List<MessageConsentReportRecord> messageRecords = new List<MessageConsentReportRecord>();

            try
            {
                var msgResults = await _messageReporter.GetMessageConsentReportAsync(reportRequest);

                if (msgResults != null)
                    messageRecords = msgResults;
            }
            catch (Exception ex)
            {
                string errMessage =
                    $"Error getting message consent report for title {titleIdGuid} from {startTime} to {endTime}";
                isError = true;
                _reportLogger.LogError(ex, errMessage);
            }


            if (isError)
                return new JsonHttpStatusResult(null, HttpStatusCode.InternalServerError);

            bool isCsvOutput = false;
            if (Request.Headers.ContainsKey("Accept"))
            {
                string acceptVal = Request.Headers["Accept"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(acceptVal))
                    isCsvOutput = acceptVal.Equals("text/csv");
            }

            if (isCsvOutput)
            {
                return WriteMessageReportCsvExport(messageRecords);


            }


            return new OkObjectResult(messageRecords);

        }

        private IActionResult WriteMessageReportCsvExport(List<MessageConsentReportRecord> messageRecords)
        {

            byte[] memOutput = null;

            using (MemoryStream stream = new MemoryStream())
            {

                using (StreamWriter writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
                {
                    csv.Configuration.ShouldQuote = (x, cont) => { return false; };
                    csv.Configuration.RegisterClassMap<MessageConsentReportRecordMap>();
                    csv.WriteRecords(messageRecords);
                    writer.Flush();
                }
                stream.Flush();

                memOutput = stream.ToArray();
            }
            //      writer.Write("Hello, World!");
       //     writer.Flush();
       //     stream.Position = 0;


            string fileName = "msgexport.csv";


            return File(memOutput, "text/csv", fileName);



        }
    }
}
