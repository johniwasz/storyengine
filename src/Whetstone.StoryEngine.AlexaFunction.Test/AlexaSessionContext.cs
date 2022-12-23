using System;
using System.Collections.Generic;
using System.Text;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;
using Whetstone.Alexa;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class AlexaSessionContext
    {


        internal static readonly string TestAdventureId = "amzn1.ask.skill.75bd5c59-177e-4552-ba62-d861c2782cc1";

        
        internal static readonly string AnimalFarmPIId = "amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0";
        internal static readonly string ClinicalTrialId = "amzn1.ask.skill.3ec17474-1c7f-4625-92ee-fb8b5505bc48";
        

        internal static readonly string ProdClinicalTrialId = "amzn1.ask.skill.3ec17474-1c7f-4625-92ee-fb8b5505bc48";

                                              
        internal static readonly string WhetstoneDevSkillId = "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f";

        internal static readonly string DiscountCouponFinderId = "amzn1.ask.skill.968568f8-cea6-4019-a31d-0f810c4f64c8";

        internal static readonly string ClinicalTrialTestId = "amzn1.ask.skill.09f23685-3492-4640-8995-26840fdec57c";

        internal static readonly string AnimalFarmTestId = "amzn1.ask.skill.46ecaa2c-1a03-4600-9903-854e33798879";

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
            get { return _userId;  }
            set { _userId = value; }

        }

        public string SessionId
        {
            get { return _sessionId;  }

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
            // The CanFulfillIntent does not include users or a session id.
            req.Session = GetCanFulfillSession();
            req.Version = "1.0";
            req.Request = GetRequestAttributes(RequestType.CanFulfillIntentRequest, intentName, slotValues);
            return req;

        }

        public AlexaRequest CreateIntentRequest(string intentName, Dictionary<string, string> slotValues, Dictionary<string, dynamic> sessionAttribs)
        {

            AlexaRequest req = new AlexaRequest();


            if (sessionAttribs != null)
            {
                req.Session = GetSession(sessionAttribs);
            }
            else
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


        private AlexaSessionAttributes GetSession(Dictionary<string, dynamic> sessionAttribs)
        {
            AlexaSessionAttributes retAttribs = GetSession();
            retAttribs.Attributes = sessionAttribs;
            return retAttribs;
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

        private AlexaSessionAttributes GetCanFulfillSession()
        {
            AlexaSessionAttributes session = new AlexaSessionAttributes();
            session.New = _isNewSession;
            session.Application = new ApplicationAttributes();
            session.Application.ApplicationId = _applicationId;


            return session;

        }


        //{
        //    "session": {
        //        "new": true,
        //        "sessionId": "SessionId.77a9ab98-075e-4c37-8af2-a9b6857ecdde",
        //        "application": {
        //            "applicationId": "amzn1.ask.skill.d03659bc-3e22-40b0-a85e-63d4f8c533b4"
        //        },
        //        "attributes": {},
        //        "user": {
        //            "userId": "amzn1.ask.account.AGLBOTXJWVQYVNWTY6FJ3OZKWF7QSV2JVOTZQ5O7F35MZVK4RJHFHN22HDNVKZWD5C4UZKXIHABXCYZJX7FEKSTIQ4TWKH2HPPPEITVQCU2JDPOICMP6NAVFTSGC7NWZ5Z4W7L3BZJPQBNA7HXTO7QVI2BSQBNWNBRJICU3NBZ4XLIMMYUT5PPESHWGCK5UGJT2PY7B3HA4FWII"
        //        }
        //    },
        //    "request": {
        //        "type": "LaunchRequest",
        //        "requestId": "EdwRequestId.70b4076c-dba9-414c-910f-b9abaae0d98e",
        //        "locale": "en-US",
        //        "timestamp": "2017-12-17T17:03:07Z"
        //    },
        //    "context": {
        //        "AudioPlayer": {
        //            "playerActivity": "IDLE"
        //        },
        //        "System": {
        //            "application": {
        //                "applicationId": "amzn1.ask.skill.d03659bc-3e22-40b0-a85e-63d4f8c533b4"
        //            },
        //            "user": {
        //                "userId": "amzn1.ask.account.AGLBOTXJWVQYVNWTY6FJ3OZKWF7QSV2JVOTZQ5O7F35MZVK4RJHFHN22HDNVKZWD5C4UZKXIHABXCYZJX7FEKSTIQ4TWKH2HPPPEITVQCU2JDPOICMP6NAVFTSGC7NWZ5Z4W7L3BZJPQBNA7HXTO7QVI2BSQBNWNBRJICU3NBZ4XLIMMYUT5PPESHWGCK5UGJT2PY7B3HA4FWII"
        //            },
        //            "device": {
        //                "supportedInterfaces": {}
        //            }
        //        }
        //    },
        //    "version": "1.0"
        //}
    }
}
