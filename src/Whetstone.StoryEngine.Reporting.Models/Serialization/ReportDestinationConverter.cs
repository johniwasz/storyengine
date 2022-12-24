using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models.Serialization
{
    public class ReportDestinationTypeConverter : JsonStoryConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return (typeof(ReportDestinationBase).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<ReportDestinationBase>();

            if (jo.ContainsKey("destinationType"))
            {
                JToken token = jo["destinationType"];
                string enumText = token.Value<string>();

                if (Enum.TryParse(enumText, true, out ReportDestinationType destinationType))
                {


                    switch (destinationType)
                    {

                        case ReportDestinationType.SftpEndpoint:
                            return ConvertObject<ReportDestinationSftp, ReportDestinationBase>(jo.ToString(), serializer,
                                conResolver);

                        //case InventoryItemType.Multi:
                        //    return ConvertObject<MultiItem, InventoryItemBase>(jo.ToString(), serializer,
                        //        conResolver);


                        default:
                            throw new JsonSerializationException($"Unrecognized ReportDestinationType {enumText}");
                    }

                }

            }
            else
                throw new JsonSerializationException("ReportDestinationType specifier not found");

            throw new JsonSerializationException("Unexpected condition deserializing ReportDestinationBase");

        }




        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }
}
