using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using System.Linq;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Models.Integration;
using Amazon.Lambda.Model;
using Amazon.Lambda;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Repository.Actions;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Environment = System.Environment;
using Amazon.XRay.Recorder.Core;

namespace Whetstone.StoryEngine.Repository
{

    public class StoryRequestProcessor : BaseStoryRequestProcessor
    {
        private readonly ITitleReader _titleRep;
        private readonly ISkillCache _skillCache;
        private readonly Func<NodeActionEnum, INodeActionProcessor> _actionProcessorFunc;
        private readonly IAppMappingReader _appMappingReader;
        private readonly ILogger<StoryRequestProcessor> _logger;


        public StoryRequestProcessor(IAppMappingReader appMappingReader, ITitleReader titleRep, Func<UserRepositoryType, IStoryUserRepository> userRepFunc,
                                     ISessionStoreManager sessionStore, ISkillCache skillCache,
                                     Func<NodeActionEnum, INodeActionProcessor> actionProcessorFunc, IOptions<EnvironmentConfig> envConfig,
                                     ILogger<StoryRequestProcessor> logger) : base(userRepFunc, sessionStore, envConfig, logger)
        {
            _titleRep = titleRep ?? throw new ArgumentNullException(nameof(titleRep));
            _skillCache = skillCache ?? throw new ArgumentNullException(nameof(skillCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionProcessorFunc = actionProcessorFunc ??
                                   throw new ArgumentNullException(nameof(actionProcessorFunc));
            _appMappingReader = appMappingReader ??
                                throw new ArgumentNullException(nameof(appMappingReader));
        }


        public override async Task<CanFulfillResponse> CanFulfillIntentAsync(StoryRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.SessionContext == null)
                throw new ArgumentNullException(nameof(request) , "SessionContext property cannot be null");

            bool isRequestProcessed = false;
            string engineError = null;
            Stopwatch fulfillIntentDuration = new Stopwatch();

            // Retrieves the title version from the dynamo db. This should never go directly against the database.
            request.SessionContext.TitleVersion = await GetTitleIdAsync(request.Client, request.ApplicationId, request.Alias);

            // Getting the current user assigns the engine user id.
            DataTitleClientUser curUser = await GetCurrentUser(request);


            request.SessionContext.EngineUserId = curUser?.Id;
            fulfillIntentDuration.Start();

            string applicationId = request.ApplicationId;
            string titleId = request.SessionContext.TitleVersion.ShortName;
            CanFulfillResponse canFulfillIntent = null;
            _logger.LogInformation($"Processing CanFulfillIntent for user id '{request.UserId}', session '{request.SessionId}', request id '{request.RequestId}'");

            try
            {

                if (string.IsNullOrWhiteSpace(titleId))
                {
                    string errorMsg = $"No application mapping found for applicationid {applicationId} in CanFulfillIntentAsync";
                    _logger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                _logger.LogInformation($"Found title {titleId} and version {request.SessionContext.TitleVersion.Version} for applicationid {applicationId} in CanFulfillIntentAsync");

                bool intentSupportsFulfill = false;

     
                if (request.RequestType != StoryRequestType.CanFulfillIntent)
                    throw new ArgumentException("StoryRequestType must be CanFulfillIntent request type");

                var storyIntent = await _titleRep.GetIntentByNameAsync(request.SessionContext.TitleVersion, request.Intent);

                if (storyIntent != null)
                {
                    intentSupportsFulfill = storyIntent.SupportsNamelessInvocation.GetValueOrDefault(false);
                }

                if (intentSupportsFulfill)
                {
                    _logger.LogInformation($"CanFulfill {request.Intent} intent request invoked. Intent supported.");
                    canFulfillIntent = new CanFulfillResponse {CanFulfill = YesNoMaybeEnum.Yes};

                    // If the intent request has slots associated, then match the slots.

                    if ((request.Slots?.Any()).GetValueOrDefault(false))
                    {

                        canFulfillIntent.SlotFulFillment = new Dictionary<string, SlotCanFulFill>();
                        // match the slots.
                        if (storyIntent.CanInvokeSlotValidator != null && storyIntent.CanInvokeSlotValidator is ExternalFunctionAction)
                        {
                            var slotValidator = (ExternalFunctionAction)storyIntent.CanInvokeSlotValidator;

                            SearchRequest slotReq = new SearchRequest
                            {
                                RequestType = SearchRequestType.ValidateCrumbs,
                                Crumbs = new List<IStoryCrumb>()
                            };
                            foreach (var slot in request.Slots)
                            {
                                // Check in cache.
                                SlotCanFulFill canFulfill = await _skillCache.GetCacheValueAsync<SlotCanFulFill>(applicationId, $"canfulfill:{slot.Key}:{slot.Value}");

                                if (canFulfill == null)
                                {
                                    SelectedItem selItem = new SelectedItem
                                    {
                                        Name = slot.Key,
                                        Value = slot.Value
                                    };
                                    slotReq.Crumbs.Add(selItem);
                                }
                                else
                                {
                                    canFulfillIntent.SlotFulFillment.Add(slot.Key, canFulfill);
                                }

                            }

                            if (slotReq.Crumbs.Any())
                            {
                                ExternalFunctionResult extResult = await ProcessExternalCanValidateAsync(slotReq, slotValidator);
                                if (extResult.SupportedSlots != null)
                                {
                                    foreach (string key in extResult.SupportedSlots.Keys)
                                    {
                                        SlotCanFulFill canFulFillVal = extResult.SupportedSlots[key];
                                        await _skillCache.SetCacheValueAsync<SlotCanFulFill>( applicationId, $"canfulfill: {key}:{canFulFillVal.Value}", canFulFillVal);

                                        canFulfillIntent.SlotFulFillment.Add(key, canFulFillVal);
                                    }
                                }
                            }
                        }
                        else
                        {


                            // Use SlotMappingsByName since it is reading from the file and not from the database right now.
                            if ((storyIntent.SlotMappingsByName?.Any()).GetValueOrDefault(false))
                            {

                                foreach (var slot in request.Slots)
                                {
                                    string foundSlot = null;

                                    if (storyIntent.SlotMappingsByName.ContainsKey(slot.Key))
                                    {
                                        foundSlot = storyIntent.SlotMappingsByName[slot.Key];
                                    }

                                    if (foundSlot != null)
                                    {
                                        SlotCanFulFill canFulFillSlot = new SlotCanFulFill();

                                        List<SlotType> slotTypes = await _titleRep.GetSlotTypes(request.SessionContext.TitleVersion);

                                        canFulFillSlot.CanFulfill = YesNoMaybeEnum.No;

                                        if ((slotTypes?.Any()).GetValueOrDefault(false))
                                        {
                                            SlotType foundType = slotTypes.FirstOrDefault(x => x.Name.Equals(foundSlot, StringComparison.OrdinalIgnoreCase));

                                            if (foundType != null)
                                            {

                                                List<string> matchedValues = foundType.GetMatchedValues(slot.Value);

                                                if ((matchedValues?.Any()).GetValueOrDefault(false))
                                                    canFulFillSlot.CanFulfill = YesNoMaybeEnum.Yes;
                               
                                            }
                                        }


                                        canFulFillSlot.CanUnderstand = YesNoMaybeEnum.Yes;
                                        canFulfillIntent.SlotFulFillment.Add(slot.Key, canFulFillSlot);

                                    }
                                    else
                                    {
                                        SlotCanFulFill notFoundSlot = new SlotCanFulFill
                                        {
                                            CanFulfill = YesNoMaybeEnum.No,
                                            CanUnderstand = YesNoMaybeEnum.No
                                        };
                                        canFulfillIntent.SlotFulFillment.Add(slot.Key, notFoundSlot);
                                    }
                                }
                            }


                            else
                            {
                                // none of the slots match.
                                foreach (var slot in request.Slots)
                                {
                                    SlotCanFulFill notFoundSlot = new SlotCanFulFill
                                    {
                                        CanFulfill = YesNoMaybeEnum.No,
                                        CanUnderstand = YesNoMaybeEnum.No
                                    };
                                    canFulfillIntent.SlotFulFillment.Add(slot.Key, notFoundSlot);
                                }
                            }
                        }
                    }

                    isRequestProcessed = true;
                }
                else
                {
                    _logger.LogInformation($"CanFulfill {request.Intent} intent request invoked. Intent not supported.");

                  
                }
            }
            catch(Exception ex)
            {
                AWSXRayRecorder.Instance.AddException(ex);
                StringBuilder errText = new StringBuilder();
                errText.AppendLine($"Error on can fulfill request from {request.ApplicationId}, client {request.Client}, locale {request.Locale} mapped to {request.SessionContext.TitleVersion.ShortName}, version {request.SessionContext.TitleVersion.Version}");
                errText.AppendLine($"Intent: {request.Intent}");

                if ((request.Slots?.Any()).GetValueOrDefault(false))
                {
                    foreach (var slot in request.Slots)
                    {
                        errText.AppendLine($"  Slot: {slot.Key}, {slot.Value}");
                    }
                }
                errText.AppendLine();

                errText.AppendLine(ex.ToString());
                engineError = errText.ToString();

                _logger.LogError(engineError);


            }


            if(!isRequestProcessed || canFulfillIntent == null)
            {
                canFulfillIntent = new CanFulfillResponse
                {
                    CanFulfill = YesNoMaybeEnum.No
                };

                if ((request.Slots?.Any()).GetValueOrDefault(false))
                {
                    canFulfillIntent.SlotFulFillment = new Dictionary<string, SlotCanFulFill>();

                    foreach (var slot in request.Slots)
                    {
                        SlotCanFulFill canFill = new SlotCanFulFill
                        {
                            CanUnderstand = YesNoMaybeEnum.No,
                            CanFulfill = YesNoMaybeEnum.No
                        };

                        canFulfillIntent.SlotFulFillment.Add(slot.Key, canFill);
                    }
                }              
            }
            



            fulfillIntentDuration.Stop();
            canFulfillIntent.ProcessDuration = fulfillIntentDuration.ElapsedMilliseconds;


            fulfillIntentDuration.Restart();
            fulfillIntentDuration.Stop();
            _logger.LogInformation($"Session log time (in milliseconds): {fulfillIntentDuration.ElapsedMilliseconds}");

            if (engineError != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(engineError);

                sb.AppendLine("-------------------");

                sb.Append(_logger.ToString());

                engineError = sb.ToString();
            }

            canFulfillIntent.EngineErrorText = engineError;




            return canFulfillIntent;
        }


        public override async Task<StoryResponse> ProcessStoryRequestAsync(StoryRequest request)
        {
            return await AWSXRayRecorder.Instance.TraceMethodAsync("ProcessStoryRequestAsync", async () => await ProcessStoryRequestInternalAsync(request));

        }
        private async Task<StoryResponse> ProcessStoryRequestInternalAsync(StoryRequest request)
        {

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.SessionContext == null)
                throw new ArgumentNullException(nameof(request), "SessionContext property cannot be null");

            if (string.IsNullOrWhiteSpace(request.ApplicationId))
                throw new ArgumentNullException(nameof(request), "ApplicationId property cannot be null or empty");

            Stopwatch processWatch = new Stopwatch();
            processWatch.Start();
            StoryResponse result = new StoryResponse();
            string applicationId = request.ApplicationId;
            SessionStartType? startType;

            List<Exception> exceptions = new List<Exception>();
            StoryRequestType reqType = request.RequestType;
            DataTitleClientUser curUser;


            // Retrieves the title version from the dynamo db. This should never go directly against the database.
            request.SessionContext.TitleVersion = await GetTitleIdAsync(request.Client, request.ApplicationId, request.Alias);

            if (request.SessionContext.TitleVersion != null)
            {
                AWSXRayRecorder.Instance.AddAnnotation("title", request.SessionContext.TitleVersion.ShortName);
                AWSXRayRecorder.Instance.AddAnnotation("titleversion", request.SessionContext.TitleVersion.Version);
                AWSXRayRecorder.Instance.AddAnnotation("titleid", request.SessionContext.TitleVersion.TitleId.GetValueOrDefault().ToString());
            }

            curUser = await GetCurrentUser(request);

            string titleId = request.SessionContext.TitleVersion.ShortName;

            _logger.LogInformation($"Processing standard request for user id '{request.UserId}', session '{request.SessionId}', request id '{request.RequestId}'");

            if (string.IsNullOrWhiteSpace(titleId))
            {
                string errorMsg = $"No application mapping found for applicationid {applicationId} in ProcessStoryRequestAsync";
                _logger.LogError(errorMsg);
                throw new Exception(errorMsg);
            }

            _logger.LogInformation($"Found title {titleId} for applicationid {applicationId}");

            if (curUser != null)
                curUser.TitleId = request.SessionContext.TitleVersion.TitleId.GetValueOrDefault();

            if (request.IsNewSession.GetValueOrDefault(false))
            {

                _logger.LogInformation($"Is new session");
                startType = await SessionStoreManager.SaveSessionStartTypeAsync(request);
                if (startType == SessionStartType.IntentStart)
                {
                    // Since they are coming off the starting intent, default to the choices on the WelcomeNode.
                    string startNode = await _titleRep.GetStartNodeNameAsync(request.SessionContext.TitleVersion, curUser.IsNew);
                    curUser.CurrentNodeName = startNode;

                    if (request.RequestType == StoryRequestType.Launch)
                        reqType = StoryRequestType.Intent;
                }               
            }
            else
            {
                _logger.LogInformation($"Not a new session");
                startType = await SessionStoreManager.GetSessionStartTypeAsync(request);
                _logger.LogInformation($"Retrieved session start type: {startType}");
            }


            bool isPrivacyEnabled = await  _titleRep.IsPrivacyLoggingEnabledAsync(request.SessionContext.TitleVersion);

            StringBuilder prenodeActionLog = new StringBuilder();

            // the intent is null when launching the skill
            if (!string.IsNullOrWhiteSpace(request.Intent))
            {
                // Process action on intent
                var storyIntent = await _titleRep.GetIntentByNameAsync(request.SessionContext.TitleVersion, request.Intent);


                if ((storyIntent?.Actions?.Any()).GetValueOrDefault(false))
                {
                    foreach (NodeActionData actionBase in storyIntent.Actions)
                    {
                        string processActionResult =  await ProcessActionAsync(curUser.CurrentNodeName, curUser, actionBase, request);
                        prenodeActionLog.AppendLine(processActionResult);
                    }
                }
            }

           
            try
            {
                if (!string.IsNullOrWhiteSpace(titleId) &&  curUser != null)
                {
                    // BUGBUG:TODO - Add handler for Pause
                    switch (reqType)
                    {
                        case StoryRequestType.Launch:
                            result = await GetStartResponseAsync(request, applicationId, curUser);
                            break;
                        case StoryRequestType.Begin:
                            result = await GetBeginResponseAsync(request, applicationId, curUser);
                            break;
                        case StoryRequestType.Repeat:
                            result = await GetRepeatResponseAsync(request, curUser);
                            break;

                        case StoryRequestType.Resume:
                            result = await GetResumeResponseAsync(request, curUser)
                                        ?? await GetRepeatResponseAsync(request, curUser);
                            break;
                        case StoryRequestType.Pause:
                            result = await GetIntentResponseAsync(curUser, request);
                            break;
                        case StoryRequestType.Intent:
                            result = await GetIntentResponseAsync(curUser, request);
                            break;
                        case StoryRequestType.Stop:
                            result = await GetStopNodeAsync(curUser, request);
                            break;
                        case StoryRequestType.Help:
                            result = await GetHelpNodeAsync(curUser, request);
                            break;
                        case StoryRequestType.Reprompt:
                            result = await GetRepromptResponseAsync( request, curUser);
                            break;
                    }
                }
            }
            catch(AggregateException aggEx)
            {
                exceptions.AddRange(aggEx.InnerExceptions);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);                
            }

            if (!exceptions.Any())
            {

                try
                {
                    if (result != null && curUser != null)
                    {
                        curUser.CurrentNodeName = result.NodeName;
                    }

                    // Trim the response based on conditions.
                    IEnumerable<SpeechFragment> clientFrag = result?.LocalizedResponse?.SpeechResponses;
                    if (clientFrag != null)
                    {
                        result.LocalizedResponse.SpeechResponses = 
                            await ApplyFragmentConditionsAsync(clientFrag, curUser, request.SessionContext.TitleVersion, _titleRep);
                    }


                    IEnumerable<SpeechFragment> repromptFrags = result?.LocalizedResponse?.RepromptSpeechResponses;                 
                    if (repromptFrags != null)
                    {
                        result.LocalizedResponse.RepromptSpeechResponses =
                            await ApplyFragmentConditionsAsync(repromptFrags, curUser, request.SessionContext.TitleVersion, _titleRep);
                    }



                    StringBuilder postNodeActionLog = new StringBuilder();
                    if (result?.Actions != null)
                    {
                        string returnNodeName = result.NodeName;

                        foreach (var action in result.Actions)
                        {
                            string postMessage = await ProcessActionAsync(returnNodeName, curUser, action, request);

                            if (!string.IsNullOrWhiteSpace(postMessage))
                                postNodeActionLog.AppendLine(postMessage);
                        }
                    }

                    if(!request.IsPingRequest.GetValueOrDefault(false))
                    {
                        await StoryUserRepository.SaveUserAsync(curUser);
                    }

                    // If this is a single request, then make sure to end the session if there is no additional search result.
                    StoryType storyType = await _titleRep.GetStoryTypeAsync( request.SessionContext.TitleVersion);

                    if (storyType == StoryType.SingleRequest)
                    {
                        if (startType == SessionStartType.IntentStart)
                        {
                            // If the request is not valid, then a help response was issued and the session needs to stay active.
                            if (result?.HasNextAction == true || result?.IsRequestValid == false)
                            {
                                result.ForceContinueSession = true;
                                _logger.LogInformation("Single direct request returned more than one response. The session must stay open to request a handle a next intent.");
                            }
                            else if (result?.IsRequestValid == false)
                            {
                                result.ForceContinueSession = true;
                                _logger.LogInformation("Invalid request received on launch. The session must stay open to allow the user to make a second request.");

                            }
                            else
                            {
                                result.ForceContinueSession = false;
                                _logger.LogInformation("Single direct request returned a single response. Do not leave the session open.");
                            }
                        }
                        else
                        {
                            // if the response is forcing session termination, but there are still 
                            // choices available, then keep the session open.

                            //    End the session if there are no more choices presented.
                            result.ForceContinueSession =
                                await GetForceSessionByClientChoicesAsync(request.SessionContext.TitleVersion, result.Choices, request.Slots, curUser.GetConditionInfo())
                                .ConfigureAwait(false);

                        }
                    }
                    else if (storyType == StoryType.AppExperience)
                    {
                        // if the response is forcing session termination, but there are still 
                        // choices available, then keep the session open.
                        //    End the session if there are no more choices presented.
                        if (request.RequestType == StoryRequestType.Intent)
                        {
                            result.ForceContinueSession =
                                await GetForceSessionByClientChoicesAsync(request.SessionContext.TitleVersion, result.Choices, request.Slots, curUser.GetConditionInfo())
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        throw new Exception("Unexpected story type ");
                    }


                    string preNodeActionLogText = prenodeActionLog.ToString();
                    if (!string.IsNullOrWhiteSpace(preNodeActionLogText))
                        result.PreNodeActionLog = preNodeActionLogText;

                    string postNodeActionLogText = postNodeActionLog.ToString();

                    if (!string.IsNullOrWhiteSpace(postNodeActionLogText))
                    {
                        if (!string.IsNullOrWhiteSpace(result.PostNodeActionLog))
                        {
                            result.PostNodeActionLog = string.Concat(result.PostNodeActionLog, Environment.NewLine,
                                postNodeActionLogText);

                        }
                        else
                            result.PostNodeActionLog = postNodeActionLogText;
                    }
                }
                catch(Exception ex)
                {
                    exceptions.Add(ex);
                }

            }

            if(exceptions.Any())
            {
                AWSXRayRecorder.Instance.AddException(new AggregateException(exceptions));

                try
                {
                    StoryNode errorNode = await _titleRep.GetErrorNodeAsync( request.SessionContext.TitleVersion);
                    if(errorNode!=null)
                    {
                        var userConditionInfo = curUser.GetConditionInfo();
                        result = await errorNode.ToStoryResponseAsync(request.Locale, userConditionInfo, _titleRep, request.SessionContext.TitleVersion, _logger);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Error processing error node in title {request.SessionContext.TitleVersion.ShortName} and version {request.SessionContext.TitleVersion.Version}");
                }

                StringBuilder errText = new StringBuilder();

                errText.AppendLine($"Error on request from {request.ApplicationId}, client {request.Client}, locale {request.Locale} mapped to {request.SessionContext.TitleVersion.ShortName}, version {request.SessionContext.TitleVersion.Version}");

                if(curUser!=null)
                {
                    if (!string.IsNullOrWhiteSpace(curUser.UserId))
                    {
                        errText.AppendLine($"User id: {curUser.UserId}");
                    }

                    if (!string.IsNullOrWhiteSpace(curUser.CurrentNodeName))
                    {

                        if(!isPrivacyEnabled)
                            errText.AppendLine($"Current user node: {curUser.CurrentNodeName}");
                    }

                    if (!isPrivacyEnabled)
                    {
                        string titleStateErrText = ProcessStateErrorMessage(curUser.TitleState, "User Title State:");

                        if (string.IsNullOrWhiteSpace(titleStateErrText))
                            errText.Append(titleStateErrText);

                        string permTitleErrText = ProcessStateErrorMessage(curUser.PermanentTitleState, "Permanent Title State:");

                        if (string.IsNullOrWhiteSpace(permTitleErrText))
                            errText.Append(permTitleErrText);
                    }

                }

                if (!isPrivacyEnabled)
                {
                    errText.AppendLine($"Intent {request.Intent}");

                    if ((request.Slots?.Any()).GetValueOrDefault(false))
                    {
                        foreach (var slotVal in request.Slots)
                        {
                            errText.AppendLine($"{slotVal.Key} : {slotVal.Value}");
                        }

                    }
                }
                errText.AppendLine();

                foreach(Exception ex in exceptions)
                {
                    errText.AppendLine(ex.ToString());
                    errText.AppendLine();
                }

                if (result == null)
                {
                    result = new StoryResponse
                    {
                        TitleId = request.SessionContext.TitleVersion.ShortName,
                        TitleVersion = request.SessionContext.TitleVersion.Version
                    };
                }

                string errorText = errText.ToString();
                _logger.LogError(errorText);
                
                result.ForceContinueSession = false;
                result.EngineErrorText = errorText;
                
            }

            try
            {
                if (!result.ForceContinueSession)
                    await this.SessionStoreManager.ClearSessionCacheAsync(request);
            }
            catch(Exception ex)
            {
             

                if (result!=null)
                {
                    // Add this error to the end of the result.
                    string errorMsg = $"Error clearing session cache on end of session: {ex}"; 

                    if(string.IsNullOrWhiteSpace(result.EngineErrorText))
                    {
                        result.EngineErrorText = errorMsg;
                    }
                    else
                    {
                        // append the error to the end of the message
                        StringBuilder sb = new StringBuilder(result.EngineErrorText);
                        sb.AppendLine();
                        sb.Append(errorMsg);
                        result.EngineErrorText = sb.ToString();
                    }
                }

                AWSXRayRecorder.Instance.AddException(new Exception("Error clearing session cache on end of session", ex));

                _logger.LogError(ex, "Error clearing session cache on end of session");
            }

            
            await _titleRep.ClearTitleAsync( request.SessionContext.TitleVersion);

            processWatch.Stop();
            _logger.LogInformation($"Execution time (in milliseconds): {processWatch.ElapsedMilliseconds}");

            if(!string.IsNullOrWhiteSpace(result.EngineErrorText))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(result.EngineErrorText);
                sb.AppendLine("---------------------");
                sb.AppendLine(_logger.ToString());
                result.EngineErrorText = sb.ToString();
            }

            result.ProcessDuration = processWatch.ElapsedMilliseconds;
            result.SessionContext = request.SessionContext;

            
            // Clean up empty arrays
            CleanEmptyLists(result);


            return result;
        }


        /// <summary>
        /// Limit the returned choices by the type of client only. Crumbs are not considered.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GetForceSessionByClientChoicesAsync(TitleVersion titleVer, IEnumerable<Choice> choices, Dictionary<string, string> slots, ConditionInfo conditions)
        {
            List<Choice> availableChoices = new List<Choice>();

            if(choices!=null)
            {
                availableChoices = await choices.GetAvailableChoicesAsync(conditions, _titleRep, titleVer).ConfigureAwait(false);
            }

            bool forceSession = availableChoices.Any();

            return forceSession;
        }

        private void CleanEmptyLists(StoryResponse result)
        {
            result.LocalizedResponse?.SpeechResponses?.RemoveAll(x => x == null);

            result.LocalizedResponse?.RepromptSpeechResponses?.RemoveAll(x => x == null);

            result.LocalizedResponse?.GeneratedTextResponse?.RemoveAll(x => x == null);

            result.Suggestions?.RemoveAll(x => x == null);

            result.Choices?.RemoveAll(x => x == null);

            result.Actions?.RemoveAll(x => x == null);
        }

        private static string ProcessStateErrorMessage(List<IStoryCrumb> crumbs, string headingText)
        {

            StringBuilder errText = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(headingText))
                errText.AppendLine($"Heading text: {headingText}");


            if ((crumbs?.Any()).GetValueOrDefault(false))
            {
                foreach (IStoryCrumb crumb in crumbs)
                {
                    string crumbText = GetCrumbErrorText(crumb);

                    if (!string.IsNullOrWhiteSpace(crumbText))
                        errText.AppendLine($"   {crumbText}");
                }
            }


            return errText.ToString();
        }

        private static string GetCrumbErrorText(IStoryCrumb crumb)
        {
            string crumbText = null;

            if (crumb is UniqueItem)
            {
                UniqueItem uniqueItem = crumb as UniqueItem;
                crumbText = $"UniqueItem: Name - {uniqueItem.Name}";
            }
            else if (crumb is MultiItem)
            {
                MultiItem multi = crumb as MultiItem;
                crumbText = $"MultiItem: Name - {multi.Name} Count - {multi.Count}";
            }
            else if (crumb is NodeVisitRecord)
            {
                NodeVisitRecord nodeRec = crumb as NodeVisitRecord;
                crumbText = $"NodeVisitRecord: Node - {nodeRec.Name} VisitCount - {nodeRec.VisitCount}";
            }
            else if (crumb is SelectedItem)
            {
                SelectedItem selItem = crumb as SelectedItem;
                crumbText = $"SelectedItem: Name - {selItem.Name} Value - {selItem.Value}";
            }

            return crumbText;
        }

        private async Task<StoryResponse> GetStopNodeAsync(DataTitleClientUser user, StoryRequest request)
        {
            StoryResponse result;
            StoryTitle title = await _titleRep.GetByIdAsync(request.SessionContext.TitleVersion);
            StoryNode stopNode = null;
            if (!string.IsNullOrWhiteSpace(title.StopNodeName))
            {
                stopNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion, title.StopNodeName);
            }

            result = await GetNodeResponse(stopNode, request.Locale, user.GetConditionInfo(),  request.SessionContext.TitleVersion, request.ApplicationId, false);
            result.ForceContinueSession = false;
            return result;
        }

        private async Task<StoryResponse> GetHelpNodeAsync(DataTitleClientUser user, StoryRequest request)
        {
            StoryResponse result;
            StoryTitle title = await _titleRep.GetByIdAsync(request.SessionContext.TitleVersion);
            StoryNode helpNode = null;
            if (!string.IsNullOrWhiteSpace(title.HelpNodeName))
            {
                helpNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion, title.HelpNodeName);
            }


            result = await GetNodeResponse(helpNode, request.Locale, user.GetConditionInfo(), request.SessionContext.TitleVersion, request.ApplicationId, true);
            return result;
        }

        private async Task<StoryResponse> GetResumeResponseAsync(StoryRequest req, DataTitleClientUser curUser)
        {
            StoryResponse resp = null;

            string storyNodeName = curUser.StoryNodeName;

            if (!string.IsNullOrWhiteSpace(storyNodeName))
            {

                StoryNode foundNode = await _titleRep.GetNodeByNameAsync(req.SessionContext.TitleVersion, storyNodeName);
                resp = await GetNodeResponse(foundNode, curUser.Locale, curUser.GetConditionInfo(), 
                    req.SessionContext.TitleVersion, req.ApplicationId, true);
            }

            return resp;
        }



        private static async Task<List<SpeechFragment>> ApplyFragmentConditionsAsync(IEnumerable<SpeechFragment> fragments, DataTitleClientUser curUser, TitleVersion titleVersion, ITitleReader titleReader)
        {
            List<SpeechFragment> speechFragments = new List<SpeechFragment>();


            // Condition fragments can be null or empty. 
            if ((fragments?.Any()).GetValueOrDefault(false))
            {
                foreach (SpeechFragment speechFrag in fragments)
                {
                    if (speechFrag is ConditionalFragment)
                    {
                        ConditionalFragment conFrag = speechFrag as ConditionalFragment;

                        List<string> conditionNames = conFrag.Conditions;

                        // If there are no conditions defined, then conditions are implicitly met.
                        bool isConditionMet = true;

                        if (conditionNames != null)
                        {

                            isConditionMet = await conditionNames.IsConditionMetAsync(curUser.GetConditionInfo(),
                                titleReader, titleVersion);


                        }


                        if (isConditionMet)
                        {

                            var trueFrags = await ApplyFragmentConditionsAsync( conFrag.TrueResultFragments, curUser,
                                titleVersion, titleReader);
                            speechFragments.AddRange(trueFrags);
                        }
                        else
                        {
                            var falseFrags = await ApplyFragmentConditionsAsync(conFrag.FalseResultFragments, curUser,
                                titleVersion, titleReader);
                            speechFragments.AddRange(falseFrags);
                        }
                    }
                    else
                    {
                        speechFragments.Add(speechFrag);

                    }
                }
            }

            return speechFragments;
        }



        private async Task<string> ProcessActionAsync(string returnNodeName, DataTitleClientUser user, NodeActionData actionData, StoryRequest request)
        {

            string actionResultLog = null;

            string titleId = request.SessionContext.TitleVersion.ShortName;

            // Don't execute the action if there is no actionData to execute on
            // Also, don't execute the action if the user requested a repeat of the current node.
            if (actionData != null && request.RequestType != StoryRequestType.Repeat)
            {
                actionData.ParentNodeName = returnNodeName;

                if (_actionProcessorFunc == null)
                    throw new Exception($"Actions required but action processor function not provided for title {titleId} and node {returnNodeName}");

                INodeActionProcessor actionProcessor = _actionProcessorFunc(actionData.NodeAction);


                if (actionData.IsPermanent.GetValueOrDefault(false))
                {

                    List<IStoryCrumb> permCrumbs = user.PermanentTitleState ?? new List<IStoryCrumb>();


                    string titleActionLog = await actionProcessor.ApplyActionAsync(request, permCrumbs, actionData);

                    user.PermanentTitleState = permCrumbs;
                    actionResultLog = string.Concat("Permanent User State Update: ", titleActionLog);
                }
                else
                {
                    List<IStoryCrumb> crumbs = user.TitleState ?? new List<IStoryCrumb>();

                    actionResultLog = await actionProcessor.ApplyActionAsync(request, crumbs, actionData);

                    user.TitleState = crumbs;
                }
            }


            return actionResultLog;
        }

        private async Task<StoryResponse> GetRepeatResponseAsync( StoryRequest request, DataTitleClientUser user)
        {
            string userNode = user.CurrentNodeName;

            StoryNode resumeNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion, userNode);
            return await GetNodeResponse(resumeNode, request.Locale , user.GetConditionInfo(), request.SessionContext.TitleVersion, request.ApplicationId, true);

        }

        private async Task<StoryResponse> GetBeginResponseAsync( StoryRequest request, string applicationId,  DataTitleClientUser curUser)
        {
            // launch the story node.
            StoryTitle title = await _titleRep.GetByIdAsync(request.SessionContext.TitleVersion);

            StoryNode beginNode = await _titleRep.GetNodeByNameAsync( request.SessionContext.TitleVersion, title.StartNodeName);

            bool isPrivacyLoggingEnabled = await _titleRep.IsPrivacyLoggingEnabledAsync(request.SessionContext.TitleVersion);


            string titleId = request.SessionContext.TitleVersion.ShortName;

            // clear the current user info
            curUser.TitleState = new List<IStoryCrumb>();
            
            if(isPrivacyLoggingEnabled)
                _logger.LogInformation($"Starting user {curUser.UserId} on a new adventure in title {titleId} on node (redacted)");
            else
                _logger.LogInformation($"Starting user {curUser.UserId} on a new adventure in title {titleId} on node {beginNode.Name}");

            // Set the story node so the user can resume from here.
            curUser.StoryNodeName = beginNode.Name;

            return await GetNodeResponse(beginNode, curUser.Locale, curUser.GetConditionInfo(), request.SessionContext.TitleVersion, applicationId);
            
        }


        private async Task<StoryResponse> GetStartResponseAsync(StoryRequest req, string applicationId, DataTitleClientUser user)
        { 
            StoryNode returnNode = null;
            user.TitleId = req.SessionContext.TitleVersion.TitleId.GetValueOrDefault();

            StoryTitle title = await _titleRep.GetByIdAsync(req.SessionContext.TitleVersion);

            var conditionInfo = user.GetConditionInfo();

            if (user.IsNew)
            {
                _logger.LogInformation($"New user starting the skill {user.UserId}. Returning global launch node.");

                if (!string.IsNullOrWhiteSpace(title.NewUserNodeName))
                {
                    StoryNode titleLaunchNode = await _titleRep.GetNodeByNameAsync(req.SessionContext.TitleVersion, title.NewUserNodeName);

                    returnNode = titleLaunchNode 
                        ?? throw new Exception($"New user node name {title.NewUserNodeName} not found in title {req.SessionContext.TitleVersion.ShortName} and version {req.SessionContext.TitleVersion.Version}.");
                }

                // Remove the user's session history.
                user.TitleState = new List<IStoryCrumb>();
            }
            else
            {
                bool resumeFound = false;

                if (!string.IsNullOrWhiteSpace(user.StoryNodeName))
                {
                    StoryNode userCurrentNode = await _titleRep.GetNodeByNameAsync(req.SessionContext.TitleVersion, user.StoryNodeName);

                    _logger.LogInformation($"Existing user returning to skill {user.UserId}");

                    if (userCurrentNode != null)
                    {
                        var availableChoices =
                             await userCurrentNode.Choices.GetAvailableChoicesAsync( conditionInfo, _titleRep, req.SessionContext.TitleVersion);


                        if (availableChoices != null && availableChoices.Any() && !string.IsNullOrWhiteSpace(userCurrentNode.TitleId))
                        {

                            // User is resuming.
                            if (!string.IsNullOrWhiteSpace(title.ResumeNodeName))
                            {
                                StoryNode resumeNode =
                                    await _titleRep.GetNodeByNameAsync( req.SessionContext.TitleVersion, title.ResumeNodeName);


                                returnNode = resumeNode;
                            }
                           

                           
                            resumeFound = true;
                            _logger.LogInformation($"Returning user found with game in progress on title {req.SessionContext.TitleVersion.ShortName} and version {req.SessionContext.TitleVersion.Version}");
                        }
                    }
                }

                if(!resumeFound)
                {

                    if (!string.IsNullOrWhiteSpace(title.ReturningUserNodeName))
                    {
                        StoryNode welcomeBackNode = await _titleRep.GetNodeByNameAsync(req.SessionContext.TitleVersion, title.ReturningUserNodeName);

                        returnNode = welcomeBackNode;
                    }

                    _logger.LogInformation("Returning welcome back node.");
                }
            }

            returnNode.TitleId = req.SessionContext.TitleVersion.ShortName;

            var storyResponse = await GetNodeResponse(returnNode, user.Locale, conditionInfo, req.SessionContext.TitleVersion, applicationId);

            return storyResponse;
        }

        private async Task<StoryResponse> GetIntentResponseAsync(DataTitleClientUser user, StoryRequest request)
        {
            StoryResponse result = null;
            StoryNode curNode = null;
            string curNodeName = null;
            string slotValueText = null;


            bool isPrivacyLogEnabled = await _titleRep.IsPrivacyLoggingEnabledAsync(request.SessionContext.TitleVersion);


            // If the current user node name is empty, then default to the 
            // Welcome or Returning user node.
            if (!string.IsNullOrWhiteSpace(user.CurrentNodeName))
            {
                curNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion, user.CurrentNodeName);
            }


            if (!string.IsNullOrWhiteSpace(request.Intent))
            {

                if (!isPrivacyLogEnabled)
                {
                    _logger.LogInformation($"Requested intent {request.Intent}");

                    if (request.Slots != null)
                    {
                        slotValueText = string.Join(";", request.Slots.Select(x => x.Key + "=" + x.Value));
                        _logger.LogInformation($"Slot values {slotValueText}");
                    }
                }
            }
            else
                _logger.LogWarning("No intent requested");
 
        
            var condInfo = user.GetConditionInfo();




            if (curNode == null)
            {
                StoryTitle title = await _titleRep.GetByIdAsync(request.SessionContext.TitleVersion);
                StoryNode responseNode = null;

                if (!string.IsNullOrWhiteSpace(user.CurrentNodeName))
                {

                    if(isPrivacyLogEnabled)
                        _logger.LogInformation(
                            $"User current node (redacted) for user {user.UserId} on client {user.Client} not found. Redirecting to ReturningUser node");
                    else
                        _logger.LogInformation(
                            $"User current node '{user.CurrentNodeName}' for user {user.UserId} on client {user.Client} not found. Redirecting to ReturningUser node");

                    // go to returning node.
                    if (!string.IsNullOrWhiteSpace(title.ReturningUserNodeName))
                    {
                        responseNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion,
                            title.ReturningUserNodeName);
                    }
                }


                if (responseNode == null)
                {
                    _logger.LogInformation(
                        $"Current node for returning user is blank for user {user.UserId} on client {user.Client}. Redirecting to NewUserNode.");
                    // go back to the initial welcome.
                    responseNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion,
                        title.NewUserNodeName);
                }

                result = await responseNode.ToStoryResponseAsync(user.Locale, condInfo, _titleRep,
                    request.SessionContext.TitleVersion, true, _logger);


                if(string.IsNullOrWhiteSpace(request.Intent))
                    return result;

                curNode = responseNode;

            }

            curNodeName = curNode.Name;

            if (!isPrivacyLogEnabled)
                _logger.LogInformation($"Current node is {curNodeName}");

            // Get the available choices

            List<Choice> availableChoices =
                await curNode.Choices.GetAvailableChoicesAsync(condInfo, _titleRep, request.SessionContext.TitleVersion);

            //  await LoadLibraryIntents(intentRep, curNode.Choices);

            Choice selectedChoice = availableChoices.FirstOrDefault(x => x.IntentName.Equals(request.Intent, StringComparison.OrdinalIgnoreCase));
            
             
            if (selectedChoice != null)
            {
                await SessionStoreManager.ResetBadIntentCounterAsync(request);

                StringBuilder actionResultBuilder = new StringBuilder();
                if ((selectedChoice.Actions?.Any()).GetValueOrDefault(false))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    foreach (NodeActionData actionData in selectedChoice.Actions)
                    {
                        string actionResult = await ProcessActionAsync(curNodeName, user, actionData, request);
                        actionResultBuilder.AppendLine(actionResult);
                    }

                    // User conditions may have been updated based on the actions executed. Refresh the condition info.
                    condInfo = user.GetConditionInfo();
                }
                string nextNodeName =
                    await selectedChoice.GetChoiceNodeNameAsync( request.Intent, condInfo, request.Slots, _titleRep, request.SessionContext.TitleVersion);

                if (!string.IsNullOrWhiteSpace(nextNodeName))
                {

                    StoryNode nextNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion, nextNodeName);
                    if (nextNode != null)
                    {
                        result = await GetNodeResponse(nextNode, user.Locale, condInfo, request.SessionContext.TitleVersion, request.ApplicationId);

                        string actionLog = actionResultBuilder.ToString();
                        if(!string.IsNullOrWhiteSpace(actionLog))
                            result.PostNodeActionLog = actionLog;

                        user.StoryNodeName = nextNode.Name;
                    }
                    else
                    { 
                        StringBuilder builder = new StringBuilder();

                        if (isPrivacyLogEnabled)
                            builder.Append(
                                $"Selected story node (redacted) not found from choice on node (redacted) with intent (redacted) on node (redacted)");
                        else
                            builder.Append(
                                $"Selected story node {nextNodeName} not found from choice on node {curNodeName} with intent {selectedChoice.IntentName} on node {curNodeName}");


                        if (!isPrivacyLogEnabled)
                        {
                            if ((request.Slots?.Count).GetValueOrDefault(0) > 0)
                            {
                                builder.AppendLine(" with slot values: ");
                                foreach (string key in request.Slots.Keys)
                                {
                                    builder.AppendLine($"{key}: {request.Slots[key]}");
                                }
                            }
                        }

                        _logger.LogError(builder.ToString());
                    }
                }

                else
                {
                    StringBuilder builder = new StringBuilder();


                    if(isPrivacyLogEnabled)
                        builder.Append(
                            $"Destination node not resolved from choice on node (redacted) with intent (redacted) on node (redacted)");
                    else
                        builder.Append(
                            $"Destination node not resolved from choice on node {curNodeName} with intent {selectedChoice.IntentName} on node {curNode.Name}");

                    if ((request.Slots?.Count).GetValueOrDefault(0) > 0)
                    {

                        if (!isPrivacyLogEnabled)
                        {
                            builder.AppendLine(" with slot values: ");
                            foreach (string key in request.Slots.Keys)
                            {
                                builder.AppendLine($"{key}: {request.Slots[key]}");
                            }
                        }
                    }

                    _logger.LogWarning(builder.ToString());
                }
            

            }

            // If a result could not be determined, then exit. 
            if(result==null)
            {
                // The user passed a bad intent. Increment the bad intent count. 

               int badIntentCounter = await this.SessionStoreManager.IncrementBadIntentCounterAsync(request);

                if (!isPrivacyLogEnabled)
                {
                    string slotValText = string.IsNullOrWhiteSpace(slotValueText)
                   ? string.Empty
                   : string.Concat("(slotvalues: ", slotValueText, ") ");

                    _logger.LogInformation(
                        $"User intent {request.Intent} {slotValText} on current node {curNodeName} does not resolve to a valid choice. Bad intent counter: {badIntentCounter}");
                }
                else
                    _logger.LogInformation(
                      $"Bad intent counter: {badIntentCounter}");

                StoryNode badIntentNode = await _titleRep.GetBadIntentNodeAsync(request.SessionContext.TitleVersion, badIntentCounter);
                if (badIntentNode != null)
                {

                    StoryNode retNode = new StoryNode
                    {
                        Name = curNode.Name,
                        // retNode.Name = curNode.Name;
                        //  retNode.SmallImageFile = curNode.SmallImageFile;
                        //   retNode.LargeImageFile = curNode.LargeImageFile;
                        Choices = availableChoices
                    };
                    LocalizedEngineResponse badResponse = await badIntentNode.GetResponseAsync( user.Locale, condInfo, _titleRep, request.SessionContext.TitleVersion, _logger);
                    LocalizedEngineResponse origResponse = await curNode.GetResponseAsync( user.Locale, condInfo, _titleRep, request.SessionContext.TitleVersion, _logger);
                    LocalizedEngineResponse returnResponse = new LocalizedEngineResponse
                    {
                        CardResponse = badResponse.CardResponse,
                        GeneratedTextResponse = badResponse.GeneratedTextResponse,

                        // Merge the speech responses of the bad node with the reprompt of the original node
                        SpeechResponses = badResponse.SpeechResponses
                    };
                    if (origResponse?.RepromptSpeechResponses!=null)
                    {
                        returnResponse.SpeechResponses.AddRange(origResponse.RepromptSpeechResponses);
                        returnResponse.RepromptSpeechResponses = origResponse.RepromptSpeechResponses;

                    }

                    result = await GetNodeResponse(retNode, user.Locale, condInfo, request.SessionContext.TitleVersion, request.ApplicationId, true, false);

                    result.LocalizedResponse = returnResponse;
                }
                else
                {
                    result = await GetNodeResponse(curNode, user.Locale, condInfo,  request.SessionContext.TitleVersion, request.ApplicationId, true, false);
                }

            }


            // If there are no more choices in this story intent, then apply the end game options.
            if (result.Choices == null || !result.Choices.Any())
            {
                // No more choices left; this is the end, my only friend.
                StoryNode endNode = null;
                
                StoryTitle title = await _titleRep.GetByIdAsync(request.SessionContext.TitleVersion);


                // If the experience is ending, then the text on the exit node may be appended to the 
                // final node for the sake of consistent messaging. If this is enabled, then
                // get the end node and add the exit text.
                if (!string.IsNullOrWhiteSpace(title.EndOfGameNodeName) && title.AppendEndTextOnExit.GetValueOrDefault(false))
                    endNode = await _titleRep.GetNodeByNameAsync(request.SessionContext.TitleVersion, title.EndOfGameNodeName);

                if (endNode != null)
                {
                    // Append additional values to the end node.
                    // This lets the exit node append some additional messages for a uniform exit.
                    var endNodeResponse = await endNode.GetResponseAsync( user.Locale, condInfo, _titleRep, request.SessionContext.TitleVersion, _logger);

                    if (endNodeResponse?.GeneratedTextResponse != null)
                    {
                        if (result.LocalizedResponse.GeneratedTextResponse == null)
                            result.LocalizedResponse.GeneratedTextResponse = new List<TextFragmentBase>();

                        result.LocalizedResponse.GeneratedTextResponse.AddRange(endNodeResponse.GeneratedTextResponse);
                    }

                    if (endNodeResponse?.SpeechResponses != null)
                    {
                        if (result.LocalizedResponse.SpeechResponses == null)
                            result.LocalizedResponse.SpeechResponses = new List<SpeechFragment>();

                        result.LocalizedResponse.SpeechResponses.AddRange(endNodeResponse.SpeechResponses);
                    }
                }
            }
            else
            {
                result.ForceContinueSession = true;
            }


            return result;
        }

        private async Task<StoryResponse> GetNodeResponse(StoryNode node, string userLocale, ConditionInfo conditionInfo, TitleVersion titleVersion, string applicationId)
        {
            return await GetNodeResponse(node, userLocale, conditionInfo, titleVersion, applicationId, true, true);
        }

        private async Task<StoryResponse> GetNodeResponse(StoryNode node, string userLocale, ConditionInfo conditionInfo, TitleVersion titleVersion, string applicationId, bool continueSession = true, bool isRequestValid = true)
        {
            Dictionary<string, SearchResponse> searchResponses = null;
            ExternalFunctionResult extResult = null;

            bool hasNextAction = false;
            if((node.DataRetrievalActions?.Any()).GetValueOrDefault())
            {
                searchResponses = new Dictionary<string, SearchResponse>();

                foreach (DataRetrievalAction dataAction in node.DataRetrievalActions)
                {
                    if(dataAction is TableFunctionSearchAction)
                    {
                        var tableResponse =   await ProcessTableActionAsync(userLocale, conditionInfo.Crumbs, dataAction);

                        if(!string.IsNullOrWhiteSpace(tableResponse.Key))
                        {
                            searchResponses.Add(tableResponse.Key, tableResponse.Value);
                        }
                    }


                    // because the data action overrides the node response, there can be only one ExternalFunctionAction.
                    if(dataAction is ExternalFunctionAction)
                    {
                        ExternalFunctionAction externalAction = dataAction as ExternalFunctionAction;
                        extResult = await ProcessExternalActionAsync(userLocale, conditionInfo, externalAction,  applicationId);
                        if(extResult?.HasNextResult==true)
                        {
                            hasNextAction = true;
                        }

                    }                
                }
            }

            StoryResponse resp = await node.ToStoryResponseAsync(userLocale, conditionInfo, _titleRep, titleVersion, continueSession, isRequestValid, _logger);


            // If a localized response was processed from an external call, then override the
            // default response.
            if (extResult?.Response!=null)
            {
                resp.LocalizedResponse = extResult.Response;
            }

            if (resp != null)
            {
                resp.SearchResponses = searchResponses;
                resp.HasNextAction = hasNextAction;
            }
            return resp;
        }

        private async Task<ExternalFunctionResult> ProcessExternalCanValidateAsync(SearchRequest request, ExternalFunctionAction dataAction)
        {
            
            string searchReqText = request.ToJsonString();
            ExternalFunctionResult extResult = await InvokeLambdaAsync<ExternalFunctionResult>(dataAction.FunctionName, searchReqText, dataAction.Alias);
            return extResult;
        }


        private async Task<KeyValuePair<string, SearchResponse>> ProcessTableActionAsync(string userLocale, List<IStoryCrumb> storyCrumbs, DataRetrievalAction dataAction)
        {
            TableFunctionSearchAction tableAction = dataAction as TableFunctionSearchAction;
            // Invoke the lambda

            SearchRequest req = new SearchRequest
            {
                Crumbs = storyCrumbs,
                Locale = userLocale
            };

            string searchReqText = req.ToJsonString();

            SearchResponse funcResp = await InvokeLambdaAsync<SearchResponse>(tableAction.FunctionName, searchReqText, tableAction.Alias);
            KeyValuePair<string, SearchResponse> retResponse = new KeyValuePair<string, SearchResponse>(tableAction.ResultSetName, funcResp);

            return retResponse;
        }


        private async Task<ExternalFunctionResult> ProcessExternalActionAsync(string userLocale, ConditionInfo condInfo, ExternalFunctionAction dataAction, string applicationId)
        {

            SearchRequest req = new SearchRequest
            {
                Crumbs = condInfo.Crumbs,
                Locale = userLocale
            };

            // Get the requested index value if specified
            if (!string.IsNullOrWhiteSpace(dataAction.IndexItem))
            {
                if (req.Crumbs.FirstOrDefault(x =>
                 {
                     if (x is MultiItem)
                     {
                         MultiItem foundItem = x as MultiItem;

                         if (foundItem.Name.Equals(dataAction.IndexItem, StringComparison.OrdinalIgnoreCase))
                             return true;
                     }
                     return false;
                 }) is MultiItem foundCrumb)
                {
                    req.Index = foundCrumb.Count;
                }
            }


            ExternalFunctionResult extResult = null;
            string searchReqText = req.ToJsonString();



            if (dataAction.CacheResult.GetValueOrDefault(false))
            {

                string extKey = GenerateExternalFunctionKey(dataAction, req);

                extResult = await _skillCache.GetCacheValueAsync<ExternalFunctionResult>(applicationId, extKey);

                if (extResult == null)
                {

                    if (string.IsNullOrWhiteSpace(dataAction.Alias))
                    {
                        _logger.LogDebug($"External result is cacheable. Cached result not found for key {extKey}. Calling external function {dataAction.FunctionName}");
                    }
                    else
                    {
                        _logger.LogDebug($"External result is cacheable. Cached result not found for key {extKey}. calling external function {dataAction.FunctionName}, alias {dataAction.Alias}");
                    }


                    extResult = await InvokeLambdaAsync<ExternalFunctionResult>(dataAction.FunctionName, searchReqText, dataAction.Alias);
                    await _skillCache.SetCacheValueAsync(applicationId, extKey, extResult);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(dataAction.Alias))
                    {
                        _logger.LogDebug($"External function {dataAction.FunctionName} result found in cache for key {extKey}.");
                    }
                    else
                    {
                        _logger.LogDebug($"External function {dataAction.FunctionName}, alias {dataAction.Alias} result found in cache for key {extKey}.");
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dataAction.Alias))
                {
                    _logger.LogDebug($"External result not cacheable - calling external function {dataAction.FunctionName}");
                }
                else
                {
                    _logger.LogDebug($"External result not cacheable - calling external function {dataAction.FunctionName}, alias {dataAction.Alias}");
                }

                extResult = await InvokeLambdaAsync<ExternalFunctionResult>(dataAction.FunctionName, searchReqText, dataAction.Alias);
            }

            // Process the text responses into a single string.

            if (extResult?.Response is null && !string.IsNullOrWhiteSpace(dataAction.Alias))
            {
                _logger.LogError($"Result from external function {dataAction.FunctionName} with alias {dataAction.Alias} is null. This is an unexpected condition.");
            }
            else
            {
                _logger.LogError($"Result from external function {dataAction.FunctionName} with no alias is null. This is an unexpected condition.");
            }

            return extResult;
        }


        private static string GenerateExternalFunctionKey(ExternalFunctionAction dataAction, SearchRequest req)
        {

            StringBuilder keyBuilder = new StringBuilder();
            
            keyBuilder.Append(dataAction.FunctionName);
            keyBuilder.Append(":");

            if(!string.IsNullOrWhiteSpace(dataAction.Alias))
            {
                keyBuilder.Append(dataAction.Alias);
                keyBuilder.Append(":");                
            }


            if(req!=null)
            {
                keyBuilder.Append(req.ToString());
            }

            string keyVal = keyBuilder.ToString();

            return keyVal;
        }



        
        private async Task<T> InvokeLambdaAsync<T>(string functionName, string requestText, string alias)
        {
            T retVal = default;

            using (AmazonLambdaClient lamdbaClient = new AmazonLambdaClient(_endpoint))
            {

                InvokeRequest ir = new InvokeRequest
                {
                    FunctionName = functionName,
                    InvocationType = InvocationType.RequestResponse,
                    Payload = requestText
                };

                if (!string.IsNullOrWhiteSpace(alias))
                {
                    ir.Qualifier = alias;
                    _logger.LogInformation($"Invoke lambda {functionName} with qualifier {alias}. Sending: {requestText}");
                }
                else
                    _logger.LogInformation($"Invoke lambda {functionName}. Sending: {requestText}");

                InvokeResponse response = null;

                try
                {
                    response = await lamdbaClient.InvokeAsync(ir);
                }
                catch(Exception ex)
                {
                    if(!string.IsNullOrWhiteSpace(alias))
                        _logger.LogError(ex, $"Error invoking lambda {functionName} with qualifier {alias}. Sent: {requestText}");
                    else
                        _logger.LogError(ex, $"Error invoking lambda {functionName}. Sent: {requestText}");
                }

                if (response != null)
                {

                    var sr = new StreamReader(response.Payload);
                    JsonReader reader = new JsonTextReader(sr);


                    var serilizer = new JsonSerializer();
                    retVal = serilizer.Deserialize<T>(reader);
                }
              
            }

            return retVal;
        }



        private async Task<StoryResponse> GetRepromptResponseAsync( StoryRequest req, DataTitleClientUser user)
        {
            StoryNode curNode = await _titleRep.GetNodeByNameAsync(req.SessionContext.TitleVersion, user.CurrentNodeName);

            StoryResponse result =  await GetNodeResponse(curNode, user.Locale, user.GetConditionInfo(), req.SessionContext.TitleVersion, req.ApplicationId, true);

            return result;
        }

     
        public override async Task<StoryPhoneInfo> GetPhoneInfoAsync( TitleVersion titleVersion)
        {
            StoryPhoneInfo retPhoneInfo = await _titleRep.GetPhoneInfoAsync(titleVersion);
            return retPhoneInfo;
        }


        private async Task<TitleVersion> GetTitleIdAsync(Client clientType, string applicationId, string alias)
        {
            Stopwatch titleIdRetrievalTime = Stopwatch.StartNew();

            TitleVersion titleId = null;

            try
            {
                titleId = await _appMappingReader.GetTitleAsync(clientType, applicationId, alias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorEvents.TitleMappingError,
                    ex,
                    $"Error retrieving title based on applicationId {applicationId}");

            }


            if (titleId == null)
            {

                string titleError = $"No title found for application id {applicationId}";

                _logger.LogError(ErrorEvents.TitleMappingError,
                    titleError);

                throw new Exception(titleError);
            }

            if (string.IsNullOrWhiteSpace(alias))
            {

                _logger.LogInformation($"Time to retrieve TitleVersion {titleId.ShortName} {titleId.Version} for application {applicationId}: {titleIdRetrievalTime.ElapsedMilliseconds} ms");
            }
            else
            {
                _logger.LogInformation($"Time to retrieve TitleVersion {titleId.ShortName} {titleId.Version} for application {applicationId} and alias {alias}: {titleIdRetrievalTime.ElapsedMilliseconds} ms");
            }

            return titleId;
        }



    }
}
