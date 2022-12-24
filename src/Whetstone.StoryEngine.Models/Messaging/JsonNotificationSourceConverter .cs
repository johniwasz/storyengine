using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Models.Messaging
{
    public class JsonNotificationSourceConverter : JsonStoryConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (typeof(INotificationSource).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<INotificationSource>();

            if (jo.ContainsKey("notificationSourceType"))
            {

                JToken token = jo["notificationSourceType"];
                string enumText = token.Value<string>();

                NotificationSourceTypeEnum notifType;
                if (Enum.TryParse(enumText, true, out notifType))
                {
                    switch (notifType)
                    {
                        case NotificationSourceTypeEnum.InboundSmsResponse:
                            return ConvertObject<InboundSmsNotification, INotificationSource>(jo.ToString(),
                                serializer, conResolver);

                        case NotificationSourceTypeEnum.PhoneMessageAction:
                            return ConvertObject<NotificationSourcePhoneMessageAction, INotificationSource>(jo.ToString(), serializer, conResolver);
                        default:
                            throw new JsonSerializationException($"Unrecognized NotificationSourceType {enumText}");
                    }
                }
            }
            else
                throw new JsonSerializationException("notificationType specifier not found");

            throw new JsonSerializationException("Unexpected condition deserializing INotificationSource");
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
