using System;
using System.Collections.Generic;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.UnitTests
{
    public class StorySessionContext
    {



        private readonly string _sessionId;
        private string _userId;
        private readonly string _applicationId;
        private bool _isNewSession = true;
        private readonly string _locale = null;
        private readonly TitleVersion _titleVer = null;
        private int _requestCount = 0;
        private readonly Client _clientType;
        private Guid _engineSessionId;

        public StorySessionContext(TitleVersion titleVer, Client clientType)
        {
            string userId = string.Concat("amzn1.ask.account.", Guid.NewGuid().ToString("N"));

            string sessionId = string.Concat("amzn1.echo-api.session.", Guid.NewGuid().ToString("N"));

            _titleVer = titleVer;
            _sessionId = sessionId;
            _clientType = clientType;
            _engineSessionId = Guid.NewGuid();

            if (_clientType == Client.Bixby)
            {
                // do not set any user id
                _userId = null;
            }
            else
            {
                _userId = userId;
            }

            _applicationId = Guid.NewGuid().ToString("N");
            _locale = "en-US";
        }


        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }

        }

        public string SessionId
        {
            get { return _sessionId; }

        }

        public StoryRequest CreateLaunchRequest()
        {
            StoryRequest req = InitRequest();
            req.RequestType = StoryRequestType.Launch;
            _isNewSession = false;
            return req;
        }

        public StoryRequest CreateCanFulfillIntentRequest(string intentName, Dictionary<string, string> slotValues)
        {

            StoryRequest req = InitRequest();
            // The CanFulfillIntent does not include users or a session id.
            req.RequestType = StoryRequestType.CanFulfillIntent;
            req.Intent = intentName;
            req.Slots = slotValues;
            _isNewSession = false;
            return req;

        }

        public StoryRequest CreateIntentRequest(string intentName)
        {
            return CreateIntentRequest(intentName, null);

        }

        public StoryRequest CreatePauseRequest()
        {
            StoryRequest req = InitRequest();
            req.RequestType = StoryRequestType.Pause;
            return req;
        }


        public StoryRequest CreateIntentRequest(string intentName, Dictionary<string, string> slotValues)
        {

            StoryRequest req = InitRequest();

            req.RequestType = StoryRequestType.Intent;
            req.Intent = intentName;
            req.Slots = slotValues;


            _requestCount++;

            if (_requestCount > 1)
                _isNewSession = false;

            req.IsNewSession = _isNewSession;

            return req;


        }

        private StoryRequest InitRequest()
        {
            StoryRequest req = new StoryRequest
            {
                IsNewSession = _isNewSession,
                ApplicationId = _applicationId
            };

            if (_clientType == Client.Bixby)
            {
                req.IsGuest = true;
                req.UserId = null;
            }
            else
            {
                req.UserId = _userId;
            }

            req.EngineRequestId = Guid.NewGuid();

            req.SessionId = _sessionId;
            req.Client = _clientType;
            req.RequestId = string.Concat("EdwRequestId.", Guid.NewGuid().ToString());
            req.Locale = string.IsNullOrWhiteSpace(_locale) ? "en-US" : _locale;
            req.RequestTime = DateTime.UtcNow;
            req.SessionContext = new EngineSessionContext
            {
                TitleVersion = _titleVer
            };
            req.SessionContext.TitleVersion.TitleId = new Guid();
            req.SessionContext.EngineSessionId = _engineSessionId;

            return req;
        }



    }
}
