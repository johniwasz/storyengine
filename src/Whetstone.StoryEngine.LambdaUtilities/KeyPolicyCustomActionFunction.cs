using Amazon.KeyManagementService.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.LambdaUtilities.KeyPolicy;
using Whetstone.StoryEngine.LambdaUtilities.Models;
using Whetstone.StoryEngine.Models.Configuration;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Whetstone.StoryEngine.LambdaUtilities
{
    public class KeyPolicyCustomActionFunction : CustomRequestFunctionBase<KeyPolicyRequest>
    {
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public KeyPolicyCustomActionFunction()
        {
        }


        internal override async Task<RequestProcessResponse> ProcessRequestAsync(ResourceRequestType requestType, KeyPolicyRequest resourceProps, ILambdaLogger logger)
        {
            RequestProcessResponse processResponse = new RequestProcessResponse
            {
                IsProcessed = true
            };

            try
            {
                // Process a key management update.
                switch (requestType)
                {
                    case ResourceRequestType.Delete:
                        await KeyPolicyManager.DeleteKeyManagerAsync(resourceProps, logger);
                        break;
                    case ResourceRequestType.Create:
                    case ResourceRequestType.Update:
                        await KeyPolicyManager.UpdateKeyManagerAsync(resourceProps, logger);
                        break;
                }
            }
            catch (InvalidRoleException invalidRoleEx)
            {
                StringBuilder errMsg = new StringBuilder();
                errMsg.AppendLine("Error: Invalid role provided: ");
                errMsg.AppendLine(JsonConvert.SerializeObject(resourceProps));
                errMsg.AppendLine(invalidRoleEx.ToString());

                logger.LogLine(errMsg.ToString());

                processResponse.IsProcessed = false;
                processResponse.TemplateStatus = $"Invalid role: {invalidRoleEx.RoleArn}";


            }
            catch (MalformedPolicyDocumentException badPolicyEx)
            {
                StringBuilder errMsg = new StringBuilder();
                errMsg.AppendLine("Error: Malformed policy document generated from request: ");
                errMsg.AppendLine(JsonConvert.SerializeObject(resourceProps));
                errMsg.AppendLine(badPolicyEx.ToString());

                logger.LogLine(errMsg.ToString());
                processResponse.IsProcessed = false;
                processResponse.TemplateStatus = "Malformed policy document";
            }



            return processResponse;
        }
    }
}
