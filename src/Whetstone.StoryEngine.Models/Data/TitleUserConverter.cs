using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Models.Data
{
    public class TitleUserConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {

            bool isValid = sourceType == typeof(Dictionary<string, AttributeValue>) || base.CanConvertFrom(context, sourceType);



            return isValid;
        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            DataTitleClientUser clientUser = null;

            if (value is Dictionary<string, AttributeValue> attribValues)
            {
                string hashKey = null;
                if (attribValues.ContainsKey("hashKey"))
                    hashKey = attribValues["hashKey"].S;

                if (!string.IsNullOrWhiteSpace(hashKey))
                {
                    clientUser = new DataTitleClientUser { HashKey = hashKey };

                    if (attribValues.ContainsKey("clientUserId"))
                        clientUser.UserId = attribValues["clientUserId"].S;

                    if (attribValues.ContainsKey("titleId"))
                        clientUser.TitleId = Guid.Parse(attribValues["titleId"].S);

                    if (attribValues.ContainsKey("client"))
                        clientUser.Client = (Models.Client)long.Parse(attribValues["client"].N);

                    if (attribValues.ContainsKey("currentNode"))
                        clientUser.CurrentNodeName = attribValues["currentNode"].S;

                    if (attribValues.ContainsKey("storyNode"))
                        clientUser.StoryNodeName = attribValues["storyNode"].S;

                    if (attribValues.ContainsKey("createdTime"))
                        clientUser.CreatedTime = DateTime.Parse(attribValues["createdTime"].S);

                    if (attribValues.ContainsKey("lastAccessedDate"))
                        clientUser.CreatedTime = DateTime.Parse(attribValues["lastAccessedDate"].S);

                    if (attribValues.ContainsKey("locale"))
                        clientUser.Locale = attribValues["locale"].S;

                    if (attribValues.ContainsKey("id"))
                        clientUser.Id = Guid.Parse(attribValues["id"].S);

                    clientUser.TitleState = GetStateCrumbs(attribValues, "titleState");

                    clientUser.PermanentTitleState = GetStateCrumbs(attribValues, "permanentTitleState");
                }
            }

            return clientUser ?? base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {

            DataTitleClientUser clientUser = value as DataTitleClientUser;
            object retValue = null;

            if ((destinationType == typeof(Dictionary<string, AttributeValue>)) && clientUser != null)
            {
                Dictionary<string, AttributeValue> retAttribs = new Dictionary<string, AttributeValue>();

                if (clientUser.Id.HasValue)
                {
                    retAttribs.Add("id", new AttributeValue() { S = clientUser.Id.ToString() });
                }


                if (!string.IsNullOrWhiteSpace(clientUser.UserId))
                    retAttribs.Add("clientUserId", new AttributeValue() { S = clientUser.UserId });

                if (clientUser.TitleId != default(Guid))
                    retAttribs.Add("titleId", new AttributeValue() { S = clientUser.TitleId.ToString() });

                retAttribs.Add("client", new AttributeValue() { N = clientUser.Client.ToString() });

                if (!string.IsNullOrWhiteSpace(clientUser.CurrentNodeName))
                    retAttribs.Add("currentNode", new AttributeValue() { S = clientUser.CurrentNodeName });

                if (!string.IsNullOrWhiteSpace(clientUser.StoryNodeName))
                    retAttribs.Add("storyNode", new AttributeValue() { S = clientUser.StoryNodeName });

                if (clientUser.CreatedTime != default(DateTime))
                    retAttribs.Add("createdTime", new AttributeValue() { S = clientUser.CreatedTime.ToString() });

                if (clientUser.LastAccessedDate != default(DateTime))
                    retAttribs.Add("lastAccessedDate", new AttributeValue() { S = clientUser.LastAccessedDate.ToString() });

                if (!string.IsNullOrWhiteSpace(clientUser.Locale))
                    retAttribs.Add("locale", new AttributeValue() { S = clientUser.Locale });

                if (clientUser.TitleState != null)
                {
                    string titleState = JsonConvert.SerializeObject(clientUser.TitleState);
                    retAttribs.Add("titleState", new AttributeValue() { S = titleState });
                }

                if (clientUser.PermanentTitleState != null)
                {
                    string titleState = JsonConvert.SerializeObject(clientUser.PermanentTitleState);
                    retAttribs.Add("permanentTitleState", new AttributeValue() { S = titleState });
                }

            }

            // Return retAttribs if it was populated, otherwise fall back to base.ConvertTo
            return retAttribs.Count > 0 ? retAttribs : base.ConvertTo(context, culture, value, destinationType);
        }

        private List<IStoryCrumb> GetStateCrumbs(Dictionary<string, AttributeValue> dynamoObject, string attribName)
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
    }
}
