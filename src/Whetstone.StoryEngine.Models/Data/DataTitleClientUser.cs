using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Tracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Whetstone.StoryEngine.Models.Data
{

    [TypeConverter(typeof(TitleUserConverter))]
    [Table("title_clientusers")]
    public class DataTitleClientUser
    {

        public static readonly string SORT_KEY_VALUE = "user";

        private const string FIELD_HASHKEY = "gsk1";
        private const string FIELD_ID = "id";
        private const string FIELD_CLIENTUSERID = "clientUserId";
        private const string FIELD_TITLEID = "titleId";
        private const string FIELD_CLIENTTYPE = "client";
        private const string FIELD_CURRENTNODE = "currentNode";
        private const string FIELD_STORYNODE = "storyNode"; 
        private const string FIELD_CREATEDATE = "createDate";
        private const string FIELD_LASTACCESSEDDATE = "lastAccessedDate";
        private const string FIELD_LOCALE = "locale";
        private const string FIELD_TITLESTATE = "titleState";
        private const string FIELD_PERMTITLESTATE = "permTitleState";
        private const string FIELD_SORTKEY = "sortKey";
        private const string FIELD_ISGUEST = "isGuest";

        public DataTitleClientUser()
        {


        }

        /// <summary>
        /// Key used to identify record in DynamoDb. This is not propagated to the SQL store.
        /// </summary>
        [Column("hashkey", Order = 0)]
        [JsonProperty(PropertyName = "hashKey", NullValueHandling = NullValueHandling.Ignore)]
        [DynamoDBProperty(AttributeName = "gsk1")]
        [DynamoDBHashKey]
        public string HashKey { get; set; }


        [Key]
        [DynamoDBProperty(AttributeName = "id")]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id", Order = 0)]
        [DataMember]
        public Guid? Id { get; set; }


        /// <summary>
        /// Database title Id
        /// </summary>
        [DynamoDBProperty(AttributeName = "titleId")]
        [Column("titleid", Order = 1)]
        [JsonRequired]
        [DataMember]
        [JsonProperty(PropertyName = "titleId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid TitleId { get; set; }


        [IgnoreDataMember]
        [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
        public DataTitle Title { get; set; }

        /// <summary>
        /// The user id provided by the connecting service.
        /// </summary>
        /// <remarks>If the connected service is Alexa, then this is the Alexa user id.</remarks>
        [Column("clientuserid", Order = 2)]
        [DynamoDBProperty(AttributeName = "clientUserId")]
        [JsonRequired]
        [DataMember]
        [JsonProperty(PropertyName = "clientUserId")]
        public string UserId { get; set; }


        [DynamoDBProperty(AttributeName = "client")]
        [Column("client", Order = 3)]
        [JsonRequired]
        [DataMember]
        [JsonProperty(PropertyName = "client")]
        public Client Client { get; set; }

        [DynamoDBProperty(AttributeName = "locale")]
        [Column("userlocale", Order = 4)]
        [DataMember]
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [DynamoDBProperty(AttributeName = "storyNode")]
        [Column("storynodename", Order = 5)]
        [DataMember]
        [JsonProperty(PropertyName = "storyNode", NullValueHandling = NullValueHandling.Ignore)]
        public string StoryNodeName { get; set; }

        /// <summary>
        /// A unique identifier used to resume the adventure where the user left off.
        /// </summary>
        [DynamoDBProperty(AttributeName = "currentNode")]
        [Column("nodename", Order = 6)]
        [DataMember]
        [JsonProperty(PropertyName = "currentNode", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentNodeName { get; set; }


        /// <summary>
        /// First time the session connected. This is a UTC date time.
        /// </summary>
        [DynamoDBProperty(AttributeName = "createdTime")]
        [Column("createdtime", Order = 7)]
        [DataMember]
        [JsonProperty(PropertyName = "createdTime")]
        public DateTime CreatedTime { get; set; }


        /// <summary>
        /// The last time the session connected. It is a UTC date time.
        /// </summary>
        [DynamoDBProperty(AttributeName = "lastAccessedDate")]
        [Column("lastaccesseddate", Order = 8)]
        [DataMember]
        [JsonProperty(PropertyName = "lastAccessedDate")]
        public DateTime LastAccessedDate { get; set; }


        [DynamoDBProperty(AttributeName = "isGuest")]
        [Column("isguest")]
        [DataMember]
        [JsonProperty(PropertyName = "isGuest")]
        public bool? IsGuest { get; set; }


        [DynamoDBIgnore]
        [IgnoreDataMember]
        [NotMapped]
        public bool IsNew { get; set; }

        [DynamoDBProperty("titleState", typeof(SessionCrumbsConverter))]
        [NotMapped]
        [JsonProperty(PropertyName = "titleState", NullValueHandling = NullValueHandling.Ignore)]
        public List<IStoryCrumb> TitleState { get; set; }


#pragma warning disable IDE0051 // Used by the UserDataContext
        [DynamoDBIgnore]
        public string TitleStateJson
#pragma warning restore IDE0051 
        {
            get
            {
                if (TitleState == null)
                    return null;

                return JsonConvert.SerializeObject(TitleState);

            }
            set
            {
                TitleState = string.IsNullOrWhiteSpace(value)
                    ? null
                    : JsonConvert.DeserializeObject<List<IStoryCrumb>>(value);
            }
        }


        [DynamoDBProperty("permanentTitleState", typeof(SessionCrumbsConverter))]
        [DataMember]
        [NotMapped]
        [JsonProperty(PropertyName = "permanentTitleState", NullValueHandling = NullValueHandling.Ignore)]
        public List<IStoryCrumb> PermanentTitleState { get; set; }


#pragma warning disable IDE0051 // Used by the UserDataContext
        [DynamoDBIgnore]
        private string PermanentTitleStateJson
#pragma warning restore IDE0051 
        {
            get
            {
                if (TitleState == null)
                    return null;

                return JsonConvert.SerializeObject(PermanentTitleState);

            }
            set
            {
                PermanentTitleState = string.IsNullOrWhiteSpace(value)
                    ? null
                    : JsonConvert.DeserializeObject<List<IStoryCrumb>>(value);
            }
        }

        [DynamoDBIgnore]
        [JsonProperty(PropertyName = "titleUserPhones", NullValueHandling = NullValueHandling.Ignore)]
        public List<UserPhoneConsent> TitleUserPhones { get; set; }


        [DynamoDBIgnore]
        [ForeignKey("TitleUserId")]
        [JsonProperty(PropertyName = "sessions")]
        public List<EngineSession> Sessions { get; set; }

        public string GetTitleStateJson()
        {
            return this.TitleStateJson;
        }

        public string GetPermanentTitleStateJson()
        {
            return this.PermanentTitleStateJson;
        }

        public static explicit operator Dictionary<string, AttributeValue>(DataTitleClientUser clientUser)
        {


            Dictionary<string, AttributeValue> retAttribs = null;

            if (clientUser != null)
            {
                retAttribs = new Dictionary<string, AttributeValue>();

                if (!string.IsNullOrWhiteSpace(clientUser.HashKey))
                    retAttribs.Add(FIELD_HASHKEY, new AttributeValue() {S = clientUser.HashKey});

                if (clientUser.Id.HasValue)
                {
                    retAttribs.Add(FIELD_ID, new AttributeValue() {S = clientUser.Id.ToString()});
                }

                retAttribs.Add(FIELD_SORTKEY, new AttributeValue(SORT_KEY_VALUE));


                if (!string.IsNullOrWhiteSpace(clientUser.UserId))
                    retAttribs.Add(FIELD_CLIENTUSERID, new AttributeValue() {S = clientUser.UserId});

                if (clientUser.TitleId != default(Guid))
                    retAttribs.Add(FIELD_TITLEID, new AttributeValue() {S = clientUser.TitleId.ToString()});

                retAttribs.Add(FIELD_CLIENTTYPE, new AttributeValue() {S = clientUser.Client.ToString()});

                if (!string.IsNullOrWhiteSpace(clientUser.CurrentNodeName))
                    retAttribs.Add(FIELD_CURRENTNODE, new AttributeValue() {S = clientUser.CurrentNodeName});

                if (!string.IsNullOrWhiteSpace(clientUser.StoryNodeName))
                    retAttribs.Add(FIELD_STORYNODE, new AttributeValue() {S = clientUser.StoryNodeName});

                if (clientUser.CreatedTime != default(DateTime))
                    retAttribs.Add(FIELD_CREATEDATE, new AttributeValue() {S = clientUser.CreatedTime.ToString(CultureInfo.InvariantCulture) });

                if (clientUser.LastAccessedDate != default(DateTime))
                    retAttribs.Add(FIELD_LASTACCESSEDDATE,
                        new AttributeValue() {S = clientUser.LastAccessedDate.ToString(CultureInfo.InvariantCulture) });

                if (!string.IsNullOrWhiteSpace(clientUser.Locale))
                    retAttribs.Add(FIELD_LOCALE, new AttributeValue() {S = clientUser.Locale});

                if (clientUser.TitleState != null)
                {
                    string titleState = JsonConvert.SerializeObject(clientUser.TitleState);
                    retAttribs.Add(FIELD_TITLESTATE, new AttributeValue() {S = titleState});
                }

                if (clientUser.PermanentTitleState != null)
                {
                    string titleState = JsonConvert.SerializeObject(clientUser.PermanentTitleState);
                    retAttribs.Add(FIELD_PERMTITLESTATE, new AttributeValue() {S = titleState});
                }


                if (clientUser.IsGuest.HasValue)
                    retAttribs.Add(FIELD_ISGUEST, new AttributeValue() { BOOL = clientUser.IsGuest.Value });
            }

            return retAttribs;
        }

        public static explicit operator DataTitleClientUser(Dictionary<string, AttributeValue> attribValues)
        {
            DataTitleClientUser clientUser = null;

            if (attribValues != null) 
            {
                string hashKey = null;
                if (attribValues.ContainsKey(FIELD_HASHKEY))
                    hashKey = attribValues[FIELD_HASHKEY].S;

                if (!string.IsNullOrWhiteSpace(hashKey))
                {
                    clientUser = new DataTitleClientUser { HashKey = hashKey };

                    if (attribValues.ContainsKey(FIELD_CLIENTUSERID))
                        clientUser.UserId = attribValues[FIELD_CLIENTUSERID].S;

                    if (attribValues.ContainsKey(FIELD_TITLEID))
                        clientUser.TitleId = Guid.Parse(attribValues[FIELD_TITLEID].S);

                    if (attribValues.ContainsKey(FIELD_CLIENTTYPE))
                    {
                        clientUser.Client = (Models.Client) Enum.Parse(typeof(Client),  attribValues[FIELD_CLIENTTYPE].S);
                    }

                    if (attribValues.ContainsKey(FIELD_CURRENTNODE))
                        clientUser.CurrentNodeName = attribValues[FIELD_CURRENTNODE].S;

                    if (attribValues.ContainsKey(FIELD_STORYNODE))
                        clientUser.StoryNodeName = attribValues[FIELD_STORYNODE].S;

                    if (attribValues.ContainsKey(FIELD_CREATEDATE))
                        clientUser.CreatedTime = DateTime.Parse(attribValues[FIELD_CREATEDATE].S);

                    if (attribValues.ContainsKey(FIELD_LASTACCESSEDDATE))
                        clientUser.CreatedTime = DateTime.Parse(attribValues[FIELD_LASTACCESSEDDATE].S);

                    if (attribValues.ContainsKey(FIELD_LOCALE))
                        clientUser.Locale = attribValues[FIELD_LOCALE].S;

                    if (attribValues.ContainsKey(FIELD_ID))
                        clientUser.Id = Guid.Parse(attribValues[FIELD_ID].S);

                    clientUser.TitleState = GetStateCrumbs(attribValues, FIELD_TITLESTATE);

                    clientUser.PermanentTitleState = GetStateCrumbs(attribValues, FIELD_PERMTITLESTATE);

                    if (attribValues.ContainsKey(FIELD_ISGUEST))
                        clientUser.IsGuest = attribValues[FIELD_ISGUEST].BOOL;
                }
            }


            return clientUser;
        }


        private static List<IStoryCrumb> GetStateCrumbs(Dictionary<string, AttributeValue> dynamoObject, string attribName)
        {

            List<IStoryCrumb> crumbs = new List<IStoryCrumb>();

            if (dynamoObject.ContainsKey(attribName))
            {
                string titleState = dynamoObject[attribName].S;
                if (!string.IsNullOrWhiteSpace(titleState))
                {
                    titleState = Regex.Unescape(titleState);
                    crumbs = JsonConvert.DeserializeObject<List<IStoryCrumb>>(titleState);
                }
            }
            return crumbs;
        }


        public string GenerateHashKey()
        {
            string hashKey = null;
            if (string.IsNullOrWhiteSpace(this.HashKey))
            {
                hashKey = $"{this.TitleId}|{this.Client}|{this.UserId}";
            }
            else
            {
                hashKey = this.HashKey;
            }


            return hashKey;


        }
    }

    public class SessionCrumbsConverter : IPropertyConverter
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            Primitive primitive = entry as Primitive;
            if (primitive == null || !(primitive.Value is String) || string.IsNullOrEmpty((string)primitive.Value))
                throw new ArgumentOutOfRangeException();

            string rawText = primitive.AsString();

            List<IStoryCrumb> sessionCrumbs = string.IsNullOrWhiteSpace(rawText) ? null : JsonConvert.DeserializeObject<List<IStoryCrumb>>(rawText);

            return sessionCrumbs;
        }

        public DynamoDBEntry ToEntry(object value)
        {
           List<IStoryCrumb> storyCrumbs = value as List<IStoryCrumb>;
            if (storyCrumbs == null) throw new ArgumentOutOfRangeException();

            string data = JsonConvert.SerializeObject(storyCrumbs);

            DynamoDBEntry entry = new Primitive
            {
                Value = data
            };
            return entry;
        }




    }
}
