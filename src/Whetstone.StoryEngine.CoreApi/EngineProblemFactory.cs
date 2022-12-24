using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.CoreApi
{
    public class EngineProblemFactory : ProblemDetailsFactory
    {
        private readonly ApiBehaviorOptions _options;
        private readonly ILogger<EngineProblemFactory> _logger;

        /// <inheritdoc />
        public EngineProblemFactory(
            IOptions<ApiBehaviorOptions> options, ILogger<EngineProblemFactory> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            statusCode ??= StatusCodes.Status500InternalServerError;

            ProblemDetails problemDetails = null;

            var context = httpContext.Features.Get<IExceptionHandlerFeature>();

            if (context?.Error != null)
            {
                if (context.Error is AdminException adminEx)
                {
                    httpContext.Response.StatusCode = adminEx.StatusCode;

                    problemDetails = new ProblemDetails
                    {
                        Status = adminEx.StatusCode,
                        Title = adminEx.Title,
                        // Type = adminEx.Type,
                        Detail = adminEx.PublicMessage,
                        Instance = instance,
                        Extensions =
                        {

                            { "errorCode", adminEx.ErrorCode }
                        }
                    };
                }
            }

            if (problemDetails == null)
            {
                //	default exception handler
                problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Type = type,
                    Detail = detail,
                    Instance = instance,
                };
            }


            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

            return problemDetails;
        }

        /// <inheritdoc />
        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            if (modelStateDictionary == null)
            {
                throw new ArgumentNullException(nameof(modelStateDictionary));
            }

            statusCode ??= 400;

            var problemDetails = new ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode,
                Type = type,
                Detail = detail,
                Instance = instance,

            };

            if (title != null)
            {
                // For validation problem details, don't overwrite the default title with null.
                problemDetails.Title = title;
            }

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

            return problemDetails;
        }

        private void ApplyProblemDetailsDefaults(HttpContext httpContext, ProblemDetails problemDetails, int statusCode)
        {
            //problemDetails.Status ??= statusCode;
            problemDetails.Status ??= statusCode;

            if (_options.ClientErrorMapping.TryGetValue(statusCode, out var clientErrorData))
            {
                problemDetails.Title ??= clientErrorData.Title;
                problemDetails.Type ??= clientErrorData.Link;
            }

            var traceId = GetTraceId(httpContext);
            if (!string.IsNullOrWhiteSpace(traceId))
            {
                problemDetails.Extensions["traceId"] = traceId;
            }
        }


        private string GetTraceId(HttpContext context)
        {

            var traceId = Activity.Current?.Id ?? context?.TraceIdentifier;

            return traceId;
        }


        private void LogError(Exception error, string traceId)
        {
            if (error != null)
            {

                string errMsg = error.Message;

                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    errMsg = string.Concat(errMsg, $" trace id: {traceId}");
                }


                if (error is AdminException adminEx)
                {
                    _logger.Log(adminEx.LogLevel, adminEx, errMsg);
                }
                else
                {
                    _logger.LogError(error, errMsg);
                }

            }
        }

    }
}

