using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Models.Data
{
    [MessagePack.MessagePackObject()]
    [JsonObject(Title = "UserPhoneConsent", Description = "Join phone numbers with title users and record consent")]
    [Table("userphoneconsents")]
    public class UserPhoneConsent
    {
        public static readonly string SORT_KEY_PREFIX = "smsconsent#";

        private const string FIELD_CONSENTID = "gsk1";
        private const string FIELD_ID = "id";
        private const string FIELD_TITLEVERSIONID = "titleVersionId";
        private const string FIELD_CONSENTDATE = "consentDate";
        private const string FIELD_ISSMSCONSENTGRANTED = "isSmsConsentGranted";
        private const string FIELD_PHONEID = "phoneId";
        private const string FIELD_ENGINEREQUESTID = "engineRequestId";
        private const string FIELD_SORTKEY = "sortKey";
        private const string FIELD_CONSENTNAME = "consentName";

        [MessagePack.Key(0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public Guid? Id { get; set; }

        [MessagePack.Key(1)]
        [Column("titleclientuserid", Order = 1)]
        [DataMember]
        [JsonProperty(PropertyName = "titleClientUserId")]
        public Guid TitleClientUserId { get; set; }


        [MessagePack.Key(2)]
        [Column("phoneid", Order = 2)]
        [DataMember]
        [JsonProperty(PropertyName = "phoneId")]
        public Guid PhoneId { get; set; }


        /// <summary>
        /// Indicate which title version was used when consent was provided.
        /// </summary>
        [MessagePack.Key(3)]
        [Column("titleversionid", Order = 3)]
        [DataMember]
        [JsonProperty(PropertyName = "titleVersionId")]
        public Guid TitleVersionId { get; set; }


        [JsonProperty(PropertyName = "titleVersion", NullValueHandling = NullValueHandling.Ignore)]
        [MessagePack.IgnoreMember()]
        [IgnoreDataMember]
        public DataTitleVersion TitleVersion { get; set; }

        /// <summary>
        /// Name of the consent as defined in the title
        /// </summary>
        /// <remarks>A title definition could have more than one consent. This name identifies which consent this applies to. For example, they could consent to receive nofications for
        /// more than one discount offer provided by the same title.</remarks>
        [Column("name", Order = 4)]
        [MessagePack.Key(4)]
        [DataMember]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }


        [JsonProperty(PropertyName = "titleUser", NullValueHandling = NullValueHandling.Ignore)]
        [MessagePack.IgnoreMember()]
        public DataTitleClientUser TitleUser { get; set; }


        [JsonProperty(PropertyName = "phone", NullValueHandling = NullValueHandling.Ignore)]
        [MessagePack.IgnoreMember()]
        public DataPhone Phone { get; set; }


        /// <summary>
        /// Indicates if consent was provided or not.
        /// </summary>
        [Column("isconsentgiven")]
        [MessagePack.Key(5)]
        [DataMember]
        [JsonProperty(PropertyName = "isConsentGranted")]
        public bool IsSmsConsentGranted { get; set; }


        /// <summary>
        /// UTC Date time user gave consent to receive an SMS message.
        /// </summary>
        [MessagePack.Key(6)]
        [Column("smsconsentdate")]
        [DataMember]
        [JsonProperty(PropertyName = "smsConsentDate")]
        public DateTime? SmsConsentDate { get; set; }


        /// <summary>
        /// The request Id sent by the client when the user granted consent to receive an SMS.
        /// </summary>
        /// <remarks>Used to tie this back to and audit record.</remarks>
        [Column("enginerequestid")]
        [MessagePack.Key(7)]
        [DataMember]
        [JsonProperty(PropertyName = "engineRequestId")]
        public Guid EngineRequestId { get; set; }

        [JsonProperty(PropertyName = "outboundBatches", NullValueHandling = NullValueHandling.Ignore)]
        [MessagePack.IgnoreMember()]
        public List<OutboundBatchRecord> OutboundMessageBatches { get; set; }




        public static explicit operator Dictionary<string, AttributeValue>(UserPhoneConsent phoneConsent)
        {


            Dictionary<string, AttributeValue> retAttribs = null;

            if (phoneConsent != null)
            {

                Dictionary<string, AttributeValue> consentDynamoRecord = new Dictionary<string, AttributeValue>();

                // assign the user id hash key.

                AttributeValue idVal = new AttributeValue(phoneConsent.TitleClientUserId.ToString());

                consentDynamoRecord.Add(FIELD_ID, idVal);

                string sortKey = GenerateSortKey(phoneConsent);
                // Use the consent date for the sort key.
                AttributeValue sortKeyVal = new AttributeValue(sortKey);
                consentDynamoRecord.Add(FIELD_SORTKEY, sortKeyVal);


                if (phoneConsent.Id.GetValueOrDefault(default(Guid)) == default(Guid))
                {
                    phoneConsent.Id = Guid.NewGuid();

                }

                AttributeValue consentId = new AttributeValue(phoneConsent.Id.ToString());
                consentDynamoRecord.Add(FIELD_CONSENTID, consentId);

                if (!phoneConsent.SmsConsentDate.HasValue)
                    phoneConsent.SmsConsentDate = DateTime.UtcNow;

                AttributeValue consentDate = new AttributeValue(phoneConsent.SmsConsentDate.Value.ToString(CultureInfo.InvariantCulture));
                consentDynamoRecord.Add(FIELD_CONSENTDATE, consentDate);



                AttributeValue consentName = new AttributeValue(phoneConsent.Name);
                consentDynamoRecord.Add(FIELD_CONSENTNAME, consentName);


                // Is consent granted
                AttributeValue isGranted = new AttributeValue();
                isGranted.BOOL = phoneConsent.IsSmsConsentGranted;
                consentDynamoRecord.Add(FIELD_ISSMSCONSENTGRANTED, isGranted);

                AttributeValue titleVerId = new AttributeValue(phoneConsent.TitleVersionId.ToString());
                consentDynamoRecord.Add(FIELD_TITLEVERSIONID, titleVerId);


                AttributeValue phoneId = new AttributeValue(phoneConsent.PhoneId.ToString());
                consentDynamoRecord.Add(FIELD_PHONEID, phoneId);


                // This value maps directly to the request id the user provided to grant the consent.
                AttributeValue requestId = new AttributeValue(phoneConsent.EngineRequestId.ToString());
                consentDynamoRecord.Add(FIELD_ENGINEREQUESTID, requestId);




                return consentDynamoRecord;
            }

            return retAttribs;
        }

        public static explicit operator UserPhoneConsent(Dictionary<string, AttributeValue> attribValues)
        {
            UserPhoneConsent phoneConsent = null;

            if (attribValues != null)
            {
                string userId = null;
                if (attribValues.ContainsKey(FIELD_ID))
                    userId = attribValues[FIELD_ID].S;

                string consentId = null;
                if (attribValues.ContainsKey(FIELD_CONSENTID))
                    consentId = attribValues[FIELD_CONSENTID].S;

                if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(consentId))
                {
                    phoneConsent = new UserPhoneConsent { TitleClientUserId = Guid.Parse(userId), Id = Guid.Parse(consentId) };

                    if (attribValues.ContainsKey(FIELD_TITLEVERSIONID))
                        phoneConsent.TitleVersionId = Guid.Parse(attribValues[FIELD_TITLEVERSIONID].S);

                    if (attribValues.ContainsKey(FIELD_ISSMSCONSENTGRANTED))
                        phoneConsent.IsSmsConsentGranted = attribValues[FIELD_ISSMSCONSENTGRANTED].BOOL;

                    if (attribValues.ContainsKey(FIELD_PHONEID))
                        phoneConsent.PhoneId = Guid.Parse(attribValues[FIELD_PHONEID].S);

                    if (attribValues.ContainsKey(FIELD_CONSENTDATE))
                        phoneConsent.SmsConsentDate = DateTime.Parse(attribValues[FIELD_CONSENTDATE].S);

                    if (attribValues.ContainsKey(FIELD_ENGINEREQUESTID))
                        phoneConsent.EngineRequestId = Guid.Parse(attribValues[FIELD_ENGINEREQUESTID].S);

                    if (attribValues.ContainsKey(FIELD_CONSENTNAME))
                        phoneConsent.Name = attribValues[FIELD_CONSENTNAME].S;
                }
            }

            return phoneConsent;
        }


        private static string GenerateSortKey(UserPhoneConsent userPhoneConsent)
        {
            if (!userPhoneConsent.SmsConsentDate.HasValue)
                userPhoneConsent.SmsConsentDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(userPhoneConsent.Name))
                throw new ArgumentException($"{nameof(userPhoneConsent)} Name property cannot be null or empty");

            if (userPhoneConsent.PhoneId == default(Guid))
                throw new ArgumentException(
                    $"{nameof(userPhoneConsent)} PhoneId property must be set to a non-default GUID value");

            string sortKey = $"smsconsent#{userPhoneConsent.Name}#{userPhoneConsent.PhoneId.ToString()}#{userPhoneConsent.SmsConsentDate.Value.ToString(CultureInfo.InvariantCulture)}";


            return sortKey;
        }

        public static string GetSortKeyPrefix(string consentName, Guid phoneId)
        {
            string sortKeyPrefix = $"smsconsent#{consentName}#{phoneId.ToString()}#";

            return sortKeyPrefix;
        }



    }
}
