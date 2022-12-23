using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Amazon.KeyManagementService.Model;
using Amazon.Lambda.Core;
using Amazon.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Whetstone.StoryEngine.ConfigUtilities.Models;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.ConfigUtilities
{
    /// <summary>
    /// Handles common functionality for handling Cloud Formation custom actions
    /// </summary>
    public abstract class CustomRequestFunctionBase<T>
    {

        protected CustomResourceResponse InitializeResponse(CustomResourceRequest request)
        {
            CustomResourceResponse resourceResp = new CustomResourceResponse
            {
                RequestId = request.RequestId
            };

            if (string.IsNullOrWhiteSpace(request.PhysicalResourceId))
                resourceResp.PhysicalResourceId = request.LogicalResourceId;
            else
                resourceResp.PhysicalResourceId = request.PhysicalResourceId;


            resourceResp.Status = "FAILED";
            resourceResp.StackId = request.StackId;
            resourceResp.LogicalResourceId = request.LogicalResourceId;

            return resourceResp;
        }


        protected async Task SendCloudFormationStatusAsync(string responseUrl, CustomResourceResponse customResp, ILambdaLogger logger)
        {

            try
            {

                using (HttpClient putClient = new HttpClient())
                {
                    putClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string responseContents = JsonConvert.SerializeObject(customResp, Formatting.Indented);

                    logger.LogLine(responseContents);

                    var putResponse = await putClient.PutAsync(responseUrl, new StringContent(responseContents, Encoding.UTF8, "application/json"));

                    logger.LogLine(putResponse.IsSuccessStatusCode
                        ? "Put response to CloudFormation succeeded"
                        : $"Error: Put response to CloudFormation failed with {putResponse.StatusCode} and reason {putResponse.ReasonPhrase}.");
                }

            }
            catch (Exception ex)
            {
                StringBuilder errMsg = new StringBuilder();
                errMsg.AppendLine("Error: Error sending response ");
                errMsg.AppendLine(JsonConvert.SerializeObject(customResp));
                errMsg.AppendLine(ex.ToString());
                logger.LogLine(errMsg.ToString());
            }

        }



        protected static long GetNanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }



        protected T GetResourceObject(dynamic resourceProperties)
        {
            JObject rawResProps = resourceProperties as JObject;
            if (rawResProps == null)
                throw new Exception("Resource properties not found");


            T requestObj = rawResProps.ToObject<T>();


            return requestObj;
        }

        public async Task FunctionHandlerAsync(CustomResourceRequest request, ILambdaContext context)
        {
            CustomResourceResponse customResp = null;
            try
            {
                LogRuntimeLoadTime(context.Logger);

                customResp = InitializeResponse(request);
                T keyRequest = GetResourceObject(request.ResourceProperties);

                RequestProcessResponse processResp =
                    await ProcessRequestAsync(request.RequestType, keyRequest, context.Logger);

                customResp.Data = processResp.Data;

                if (processResp.IsProcessed)
                { 
                    customResp.Status = CustomResourceResponse.SUCCESS_STATUS;
                }
                else
                {
                    customResp.Status = CustomResourceResponse.FAILED_STATUS;
                    customResp.Status = processResp.TemplateStatus;
                }

            }
            catch (Exception ex)
            {
                StringBuilder errMsg = new StringBuilder();

                errMsg.AppendLine("Error: Error processing request: ");
                errMsg.AppendLine(JsonConvert.SerializeObject(request));
                errMsg.AppendLine(ex.ToString());

                context.Logger.LogLine(errMsg.ToString());
                customResp = InitializeResponse(request);
                customResp.Status = CustomResourceResponse.FAILED_STATUS;
                customResp.Reason = "Unexpected error";
            }

            await SendCloudFormationStatusAsync(request.ResponseUrl, customResp, context.Logger);

        }


        internal abstract Task<RequestProcessResponse> ProcessRequestAsync(ResourceRequestType requestType, T resourceProps,
            ILambdaLogger logger);



        protected void LogRuntimeLoadTime(ILambdaLogger logger)
        {
            string loadTimeText = Environment.GetEnvironmentVariable("_LAMBDA_RUNTIME_LOAD_TIME");
            if (!string.IsNullOrWhiteSpace(loadTimeText))
            {
                if (long.TryParse(loadTimeText, out long runtimeLoadTime))
                {
                    long curNanoTime = GetNanoTime();
                    long runtime = (curNanoTime - runtimeLoadTime) / 1000000;
                    logger.LogLine(
                        $"RuntimeLoad Time: {runtimeLoadTime}. CurNanoTime: {curNanoTime}. Runtime: {runtime}");
                }
            }
        }



    }
}
