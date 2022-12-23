using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.Alexa;

namespace Whetstone.UnitTests.MessageManagement
{
    public class AlexaSessionContext
    {



        private readonly string _sessionId;
        private string _userId;
        private readonly string _applicationId;
        private bool _isNewSession = true;
        private string _locale = null;

        private int _requestCount = 0;

        public AlexaSessionContext(string applicationId)
        {
            string userId = string.Concat("amzn1.ask.account.", Guid.NewGuid().ToString());

            string sessionId = string.Concat("amzn1.echo-api.session.", Guid.NewGuid().ToString());


            _sessionId = sessionId;

            _userId = userId;
            _applicationId = applicationId;
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

        public AlexaSessionContext(string applicationId, string sessionId)
        {
            _sessionId = sessionId;

            _userId = Guid.NewGuid().ToString();
            _applicationId = applicationId;
            _locale = "en-US";
        }


        public AlexaSessionContext(string applicationId, string sessionId, string locale)
        {
            _sessionId = sessionId;

            _userId = Guid.NewGuid().ToString();
            _applicationId = applicationId;
            _locale = locale;
        }



        public AlexaSessionContext(string applicationId, string sessionId, string locale, string userId)
        {
            _sessionId = sessionId;

            _userId = Guid.NewGuid().ToString();
            _applicationId = applicationId;
            _locale = locale;
            _userId = userId;
        }

        public AlexaRequest CreateLaunchRequest()
        {
            AlexaRequest req = new AlexaRequest();

            req.Session = GetSession();
            req.Version = "1.0";
            req.Request = GetRequestAttributes(RequestType.LaunchRequest, null, null);

            _isNewSession = false;
            return req;
        }

        public AlexaRequest CreateCanFulfillIntentRequest(string intentName, Dictionary<string, string> slotValues)
        {

            AlexaRequest req = new AlexaRequest();

            req.Session = GetSession();
            req.Version = "1.0";
            req.Request = GetRequestAttributes(RequestType.CanFulfillIntentRequest, intentName, slotValues);

            req.Session.New = _isNewSession;

            return req;

        }

        public AlexaRequest CreateIntentRequest(string intentName, Dictionary<string, string> slotValues)
        {

            AlexaRequest req = new AlexaRequest();

            req.Session = GetSession();
            req.Version = "1.0";
            req.Request = GetRequestAttributes(RequestType.IntentRequest, intentName, slotValues);

            _requestCount++;

            if (_requestCount > 1)
                _isNewSession = false;

            req.Session.New = _isNewSession;

            return req;


        }

        private RequestAttributes GetRequestAttributes(RequestType requstType, string intentName, Dictionary<string, string> slotValues)
        {

            RequestAttributes attribs = new RequestAttributes();
            if (!string.IsNullOrWhiteSpace(intentName))
            {
                attribs.Intent = new IntentAttributes();
                attribs.Intent.Name = intentName;
                if (slotValues != null && slotValues.Keys.Count > 0)
                {
                    attribs.Intent.Slots = new List<SlotAttributes>();

                    foreach (string key in slotValues.Keys)
                    {

                        SlotAttributes slotAttrib = new SlotAttributes();

                        slotAttrib.Name = key;
                        slotAttrib.Value = slotValues[key];
                        attribs.Intent.Slots.Add(slotAttrib);
                    }

                }


            }

            attribs.Type = requstType;
            attribs.RequestId = string.Concat("EdwRequestId.", Guid.NewGuid().ToString());
            attribs.Locale = string.IsNullOrWhiteSpace(_locale) ? "en-US" : _locale;
            attribs.Timestamp = DateTime.UtcNow;



            return attribs;
        }

        private AlexaSessionAttributes GetSession()
        {
            AlexaSessionAttributes session = new AlexaSessionAttributes();
            session.New = _isNewSession;
            session.Application = new ApplicationAttributes();
            session.Application.ApplicationId = _applicationId;

            session.User = new UserAttributes();
            session.User.UserId = _userId;

            session.SessionId = _sessionId;


            return session;

        }

    }
}
