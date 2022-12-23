using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class GetPersonalInfoActionProcessor : INodeActionProcessor
    {
        private ILogger<GetPersonalInfoActionProcessor> _dataLogger;


        public GetPersonalInfoActionProcessor(ILogger<GetPersonalInfoActionProcessor> logger)
        {
            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }

#pragma warning disable CS1998
        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)
#pragma warning restore CS1998
        {
            StringBuilder sb = new StringBuilder();


            if (req.Client == Client.Alexa)
            {

                if (req.SecurityInfo == null)
                {
                    string securityInfoMsg = "Security info is missing";
                    _dataLogger.LogError(securityInfoMsg);
                    sb.Append(securityInfoMsg);
                }
                else
                {

                    bool isError = false;
                    try
                    {


                        //   secInfo = (AlexaSecurityInfo)req.SecurityInfo;

                    }
                    catch (Exception ex)
                    {
                        isError = true;
                        string convError = "Error converting SecurityInfo to AlexaSecurityInfo";
                        _dataLogger.LogError(ex, convError);
                        sb.AppendLine(convError);

                    }

                    if (!isError)
                    {


                    }


                }
            }
            else
            {
                sb.AppendFormat("Getting personal data not supported for client {0}", req.Client);
            }


            return sb.ToString();
        }
    }
}
