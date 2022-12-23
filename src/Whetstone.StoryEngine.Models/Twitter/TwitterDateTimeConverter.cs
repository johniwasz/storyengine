using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class TwitterDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(DateTimeOffset))
                return true;

            if (objectType == typeof(DateTime))
                return true;

            return false;
        }

      
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string rawText = reader.Value as string;

            if (objectType == typeof(DateTimeOffset))
                return DateTimeOffset.ParseExact(rawText, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);

            if(objectType == typeof(DateTime))
                return DateTime.ParseExact(rawText, "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

            throw new Exception("Unsupported type is requested");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
