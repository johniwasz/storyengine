using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Models.Messaging
{
    public class JsonNotificationRequestConverter : JsonStoryConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return (typeof(INotificationRequest).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<INotificationRequest>();

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
                            return ConvertObject<SmsNotificationRequest, INotificationRequest>(jo.ToString(), serializer, conResolver);
                        default:
                            throw new JsonSerializationException(string.Format("Unrecognized NotificationType {0}",
                                enumText));
                    }
                }
            }
            else
                throw new JsonSerializationException("notificationType specifier not found");

            throw new JsonSerializationException("Unexpected condition deserializing INotificationRequest");
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
