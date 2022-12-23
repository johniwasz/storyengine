using Amazon.DynamoDBv2.Model;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.Models.Data
{


    /// <summary>
    /// Indicates whether a phone number results in a dispatched SMS message or not based on system configurations.
    /// The phone number CanGetSmsMessage property could be true, but the phone number can still be flagged so that the
    /// outbound SMS message is still blocked.
    /// </summary>
    public enum SystemPhoneStatus
    {
        /// <summary>
        /// Requests to send to this phone number are permitted. This is the default setting.
        /// </summary>
        SendPermitted = 1,

        /// <summary>
        /// The number may be flagged to prevent sending sms messages. For example, if the user requests an excess of discount codes,
        /// the number could be blocked.
        /// </summary>
        SendBlocked = 2,

        /// <summary>
        /// The number is used for automated testing and should not result in a message being dispatched via SMS.
        /// </summary>
        TestNumber = 3
    }




    [MessagePackObject]
    [JsonObject(Title = "PhoneNumber", Description = "Provided phone numbers")]
    [Table("phonenumbers")]
    public class DataPhone
    {

        public static readonly string SORT_KEY_VALUE = "phone";

        private const string FIELD_ID = "id";
        private const string FIELD_CARRIERCOUNTRYCODE = "carrierCountryCode";
        private const string FIELD_CARRIERERRORCODE = "carrierErrorCode";
        private const string FIELD_CARRIERNAME = "carrierName";
        private const string FIELD_CARRIERNETWORKCODE = "carrierNetworkCode";
        private const string FIELD_COUNTRYCODE = "countryCode";
        private const string FIELD_NATIONALFORMAT = "nationalFormat";

        /// <summary>
        /// This corresponds to an global secondary index on the user table
        /// </summary>
        private const string FIELD_PHONENUMBER = "gsk1";
        private const string FIELD_PHONESERVICE = "phoneService";
        private const string FIELD_PHONETYPE = "type";
        private const string FIELD_ISVERIFIED = "isVerified";
        private const string FIELD_REGISTEREDERRORCODE = "registeredErrorCode";
        private const string FIELD_CREATEDATE = "createDate";
        private const string FIELD_REGISTEREDNAME = "registeredName";
        private const string FIELD_REGISTEREDTYPE = "registeredType";
        private const string FIELD_SORTKEY = "sortKey";
        private const string FIELD_CANGETSMSMESSAGES = "canGetSmsMessages";
        private const string FIELD_URL = "url";
        private const string FIELD_SYSTEMSTATUS = "systemStatus";

        [MessagePack.Key(0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        [DataMember]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        /// <summary>
        /// Formatted phone number which includes the country code. This should be used to when sending SMS responses. Returned from the phone_number property in the Twilio response
        /// </summary>
        /// <remarks>Example format is +15551212</remarks>
        [Required]
        [MessagePack.Key(1)]
        [Column("phonenumber", Order = 1)]
        [DataMember]
        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Name of the wireless carrier. Provided by the carrier.name returned from Twilio.
        /// </summary>
        /// <remarks>For example: mobile</remarks>
        [Column("phonetype", Order = 2)]
        [MessagePack.Key(2)]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        public PhoneTypeEnum Type { get; set; }

        /// <summary>
        /// Indicates that a service was invoked which provided a response indicating whether or not the phone number is verified.
        /// </summary>
        [Column("isverified", Order = 3)]
        [MessagePack.Key(3)]
        [DataMember]
        [JsonProperty(PropertyName = "isVerified")]
        public bool IsVerified { get; set; }

        /// <summary>
        /// National format of the phone number. Provided by national_format attribute returned in the Twilio response.
        /// </summary>
        /// <remarks>Example: (215) 555-1212 for US phone numbers.</remarks>
        [Column("nationalformat", Order = 4)]
        [MessagePack.Key(4)]
        [DataMember]
        [JsonProperty(PropertyName = "nationalFormat")]
        public string NationalFormat { get; set; }




        /// <summary>
        /// Determines if the phone number can receive an SMS message or not.
        /// </summary>
        [Column("cangetsmsmessage", Order =5)]
        [MessagePack.Key(5)]
        [DataMember]
        [JsonProperty(PropertyName = "canGetSmsMessage")]
        public bool CanGetSmsMessage { get; set; }

        /// <summary>
        /// Country code associated with the phone number from the country_code in the root of the Twilio response.
        /// </summary>
        [MessagePack.Key(6)]
        [Column("countrycode", Order=6)]
        [DataMember]
        [JsonProperty(PropertyName = "countryCode", NullValueHandling = NullValueHandling.Ignore)]
        public string CountryCode { get; set; }

        /// <summary>
        /// Mobile country code of the phone number. Provided by the carrier.mobile_country_code returned from Twilio.
        /// </summary>
        [MessagePack.Key(7)]
        [Column("carriercountrycode", Order =7)]
        [DataMember]
        [JsonProperty(PropertyName = "carrierCountryCode", NullValueHandling = NullValueHandling.Ignore)]
        public string CarrierCountryCode { get; set; }


        /// <summary>
        /// Mobile network code of the phone number. Provided by the carrier.mobile_network_code returned from Twilio.
        /// </summary>
        [MessagePack.Key(8)]
        [Column("carriernetworkcode", Order =8)]
        [DataMember]
        [JsonProperty(PropertyName = "carrierNetworkCode", NullValueHandling = NullValueHandling.Ignore)]
        public string CarrierNetworkCode { get; set; }


        /// <summary>
        /// Name of the wireless carrier. Provided by the carrier.name returned from Twilio.
        /// </summary>
        /// <remarks>For example: AT&T Wireless</remarks>
        [Column("carriername", Order =9)]
        [MessagePack.Key(9)]
        [DataMember]
        [JsonProperty(PropertyName = "carrierName", NullValueHandling = NullValueHandling.Ignore)]
        public string CarrierName { get; set; }




        /// <summary>
        /// Error code returned while getting the carrier. Provided by the carrier.error_code returned from Twilio.
        /// </summary>
        [MessagePack.Key(10)]
        [Column("carriererrorcode", Order =10)]
        [DataMember]
        [JsonProperty(PropertyName = "carrierErrorCode", NullValueHandling = NullValueHandling.Ignore)]
        public string CarrierErrorCode { get; set; }


        /// <summary>
        /// Url used to obtain phone information from Twilio
        /// </summary>
        [Column("url", Order =11)]
        [MessagePack.Key(11)]
        [DataMember]
        [JsonProperty(PropertyName = "url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }



        /// <summary>
        /// Comes in from caller_name on Twilio response
        /// </summary>
        /// <remarks>Will be upper case. Can be null if only getting phone type.</remarks>
        [Column("registeredname", Order =12)]
        [MessagePack.Key(12)]
        [DataMember]
        [JsonProperty(PropertyName = "registeredName", NullValueHandling = NullValueHandling.Ignore)]
        public string RegisteredName { get; set; }


        /// <summary>
        /// Comes in from caller_type in Twilio response. 
        /// </summary>
        /// <remarks>
        /// Will be upper case. Example value is CONSUMER. Could be null if only getting mobile number type.</remarks>
        [Column("registeredtype", Order =13)]
        [MessagePack.Key(13)]
        [DataMember]
        [JsonProperty(PropertyName = "registeredType", NullValueHandling = NullValueHandling.Ignore)]
        public string RegisteredType { get; set; }


        /// <summary>
        /// Error code returned from Twilio that pertains to getting the name of user associated with the phone number. Returned from the error_code property on the RegisteredUser
        /// </summary>
        [Column("registerederrorcode", Order =14)]
        [MessagePack.Key(14)]
        [DataMember]
        [JsonProperty(PropertyName = "registeredErrorCode", NullValueHandling = NullValueHandling.Ignore)]
        public string RegisteredErrorCode { get; set; }


        /// <summary>
        /// The service used to retrieve the phone information. (e.g. Twilio, Pinpoint, etc)
        /// </summary>
        [Column("phoneservice", Order =15)]
        [MessagePack.Key(15)]
        [DataMember]
        [JsonProperty(PropertyName = "phonesService", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneService { get; set; }


        /// <summary>
        /// If the phone number is flagged as a test phone number, then it does not trigger an outbound SMS message. It is used
        /// for automated test purposes only.
        /// </summary>
        /// <remarks>
        /// The interaction flow model of the voice experience may or may not be influenced by the status of the phone number. This is different than isBlocked.
        /// The voice response should be difference depending on whether the phone number is 
        /// </remarks>
        [Column("systemstatus", Order = 16)]
        [MessagePack.Key(16)]
        [DataMember]
        [JsonProperty(PropertyName = "systemStatus")]
        public SystemPhoneStatus SystemStatus { get; set; }

        /// <summary>
        /// UTC date and time the phone number was provided.
        /// </summary>
        [Column("createdate", Order =17)]
        [MessagePack.Key(17)]
        [DataMember]
        [JsonProperty(PropertyName = "createDate")]
        public DateTime CreateDate { get; set; }


    

        [MessagePack.Key(18)]
        [JsonProperty(PropertyName = "consentRecords", NullValueHandling = NullValueHandling.Ignore)]
        public List<UserPhoneConsent> ConsentRecords { get; set; }

        /// <summary>
        /// Catalog of all the SMS batch records sent from the given phone number
        /// </summary>
        [MessagePack.IgnoreMember()]
        [JsonProperty(PropertyName = "sentFromSmsBatches", NullValueHandling = NullValueHandling.Ignore)]
        public List<OutboundBatchRecord> SentFromSmsBatches { get; set; }

        /// <summary>
        /// Catalog of all the SMS batch records sent to the given phone number
        /// </summary>
        [MessagePack.IgnoreMember()]
        [JsonProperty(PropertyName = "sentToSmsBatches", NullValueHandling = NullValueHandling.Ignore)]
        public List<OutboundBatchRecord> SentToSmsBatches { get; set; }





        public static explicit operator Dictionary<string, AttributeValue>(DataPhone phoneInfo)
        {


            Dictionary<string, AttributeValue> retAttribs = null;

            if (phoneInfo != null)
            {
                retAttribs = new Dictionary<string, AttributeValue>();

                if (phoneInfo.Id.HasValue)
                    retAttribs.Add(FIELD_ID, new AttributeValue() { S = phoneInfo.Id.Value.ToString()});

                retAttribs.Add(FIELD_PHONETYPE, new AttributeValue() { S = phoneInfo.Type.ToString() });

                retAttribs.Add(FIELD_ISVERIFIED, new AttributeValue() { BOOL = phoneInfo.IsVerified });

                retAttribs.Add(FIELD_SORTKEY, new AttributeValue(SORT_KEY_VALUE));

                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierCountryCode))
                    retAttribs.Add(FIELD_CARRIERCOUNTRYCODE, new AttributeValue() { S = phoneInfo.CarrierCountryCode });

                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierErrorCode))
                    retAttribs.Add(FIELD_CARRIERERRORCODE, new AttributeValue() { S = phoneInfo.CarrierErrorCode });

                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierName))
                    retAttribs.Add(FIELD_CARRIERNAME, new AttributeValue() { S = phoneInfo.CarrierName });

                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierNetworkCode))
                    retAttribs.Add(FIELD_CARRIERNETWORKCODE, new AttributeValue() { S = phoneInfo.CarrierNetworkCode });

                if (!string.IsNullOrWhiteSpace(phoneInfo.CountryCode))
                    retAttribs.Add(FIELD_COUNTRYCODE, new AttributeValue() { S = phoneInfo.CountryCode });

                if (!string.IsNullOrWhiteSpace(phoneInfo.NationalFormat))
                    retAttribs.Add(FIELD_NATIONALFORMAT, new AttributeValue() { S = phoneInfo.NationalFormat });

                if (!string.IsNullOrWhiteSpace(phoneInfo.PhoneNumber))
                    retAttribs.Add(FIELD_PHONENUMBER, new AttributeValue() { S = phoneInfo.PhoneNumber });

                if (!string.IsNullOrWhiteSpace(phoneInfo.PhoneService))
                    retAttribs.Add(FIELD_PHONESERVICE, new AttributeValue() { S = phoneInfo.PhoneService });

                if (!string.IsNullOrWhiteSpace(phoneInfo.RegisteredErrorCode))
                    retAttribs.Add(FIELD_REGISTEREDERRORCODE, new AttributeValue() { S = phoneInfo.RegisteredErrorCode });

                if (!string.IsNullOrWhiteSpace(phoneInfo.RegisteredName))
                    retAttribs.Add(FIELD_REGISTEREDNAME, new AttributeValue() { S = phoneInfo.RegisteredName });

                if(phoneInfo.CreateDate!=default(DateTime))
                    retAttribs.Add(FIELD_CREATEDATE, new AttributeValue() { S = phoneInfo.CreateDate.ToString(CultureInfo.InvariantCulture) });

                retAttribs.Add(FIELD_SYSTEMSTATUS, new AttributeValue() { S = phoneInfo.SystemStatus.ToString() });

                if (!string.IsNullOrWhiteSpace(phoneInfo.RegisteredType))
                    retAttribs.Add(FIELD_REGISTEREDTYPE, new AttributeValue() { S = phoneInfo.RegisteredType });

                retAttribs.Add(FIELD_CANGETSMSMESSAGES, new AttributeValue() { BOOL = phoneInfo.CanGetSmsMessage });

                if (!string.IsNullOrWhiteSpace(phoneInfo.Url))
                    retAttribs.Add(FIELD_URL, new AttributeValue() { S = phoneInfo.Url });

            }

            return retAttribs;
        }

        public static explicit operator DataPhone(Dictionary<string, AttributeValue> attribValues)
        {
            DataPhone retPhone= null;

            if (attribValues != null)
            {
                string id = null;
                if (attribValues.ContainsKey(FIELD_ID))
                    id = attribValues[FIELD_ID].S;

                if (!string.IsNullOrWhiteSpace(id))
                {
                    retPhone = new DataPhone { Id = Guid.Parse(id)};

                    if (attribValues.ContainsKey(FIELD_ISVERIFIED))
                        retPhone.IsVerified = attribValues[FIELD_ISVERIFIED].BOOL;

                    if (attribValues.ContainsKey(FIELD_CANGETSMSMESSAGES))
                        retPhone.CanGetSmsMessage = attribValues[FIELD_CANGETSMSMESSAGES].BOOL;

                    if (attribValues.ContainsKey(FIELD_PHONETYPE))
                    {
                        retPhone.Type= (PhoneTypeEnum)Enum.Parse(typeof(PhoneTypeEnum), attribValues[FIELD_PHONETYPE].S);
                    }

                    if (attribValues.ContainsKey(FIELD_SYSTEMSTATUS))
                    {
                        retPhone.SystemStatus = (SystemPhoneStatus)Enum.Parse(typeof(SystemPhoneStatus), attribValues[FIELD_SYSTEMSTATUS].S);
                    }

                    if (attribValues.ContainsKey(FIELD_CARRIERCOUNTRYCODE))
                        retPhone.CarrierCountryCode = attribValues[FIELD_CARRIERCOUNTRYCODE].S;

                    if (attribValues.ContainsKey(FIELD_CARRIERERRORCODE))
                        retPhone.CarrierErrorCode = attribValues[FIELD_CARRIERERRORCODE].S;

                    if (attribValues.ContainsKey(FIELD_CARRIERNAME))
                        retPhone.CarrierName = attribValues[FIELD_CARRIERNAME].S;

                    if (attribValues.ContainsKey(FIELD_CARRIERNETWORKCODE))
                        retPhone.CarrierNetworkCode= attribValues[FIELD_CARRIERNETWORKCODE].S;

                    if (attribValues.ContainsKey(FIELD_COUNTRYCODE))
                        retPhone.CountryCode = attribValues[FIELD_COUNTRYCODE].S;

                    if (attribValues.ContainsKey(FIELD_CREATEDATE))
                        retPhone.CreateDate = DateTime.Parse(attribValues[FIELD_CREATEDATE].S);

                    if (attribValues.ContainsKey(FIELD_NATIONALFORMAT))
                        retPhone.NationalFormat = attribValues[FIELD_NATIONALFORMAT].S;

                    if (attribValues.ContainsKey(FIELD_PHONENUMBER))
                        retPhone.PhoneNumber = attribValues[FIELD_PHONENUMBER].S;

                    if (attribValues.ContainsKey(FIELD_PHONESERVICE))
                        retPhone.PhoneService = attribValues[FIELD_PHONESERVICE].S;

                    if (attribValues.ContainsKey(FIELD_URL))
                        retPhone.Url = attribValues[FIELD_URL].S;

                    if (attribValues.ContainsKey(FIELD_REGISTEREDTYPE))
                        retPhone.RegisteredType = attribValues[FIELD_REGISTEREDTYPE].S;

                    if (attribValues.ContainsKey(FIELD_REGISTEREDNAME))
                        retPhone.RegisteredName = attribValues[FIELD_REGISTEREDNAME].S;

                    if (attribValues.ContainsKey(FIELD_REGISTEREDERRORCODE))
                        retPhone.RegisteredErrorCode = attribValues[FIELD_REGISTEREDERRORCODE].S;


                }
            }


            return retPhone;
        }


    }
}
