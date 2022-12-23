using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Models.Messaging
{
    public class JsonNotificationRecordConverter : JsonStoryConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return (typeof(IOutboundNotificationRecord).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<IOutboundNotificationRecord>();

            if (jo.ContainsKey("notificationType"))
            {

                JToken token = jo["notificationType"];
                string enumText = token.Value<string>();

                NotificationTypeEnum notifType;
                if (Enum.TryParse(enumText, true, out notifType))
                {
                    switch (notifType)
                    {
                        case NotificationTypeEnum.Sms:
                            return ConvertObject<OutboundBatchRecord, IOutboundNotificationRecord>(jo.ToString(), serializer, conResolver);
                        default:
                            throw new JsonSerializationException(string.Format("Unrecognized NotificationSourceType {0}",
                                enumText));
                    }
                }
            }
            else
                throw new JsonSerializationException("notificationType specifier not found");

            throw new JsonSerializationException("Unexpected condition deserializing INotificationRecord");
        }




        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }

    }
}
