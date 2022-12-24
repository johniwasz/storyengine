using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class TwitterMillisecondDateTimeConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {


            if (objectType == typeof(DateTime))
                return true;

            return false;
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string rawValue = reader.Value as string;

            if (objectType == typeof(DateTime))
                return (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(rawValue));

            throw new Exception("Unsupported type is requested");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
