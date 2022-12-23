using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Whetstone.StoryEngine.Serialization
{
    /// <summary>
    /// Custom JSON converter for handling special event cases.
    /// </summary>
    internal class JsonNumberToDateTimeDataConverter : JsonConverter
    {
        private static readonly TypeInfo DATETIME_TYPEINFO = typeof(DateTime).GetTypeInfo();
        private static readonly DateTime EPOCH_DATETIME = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return JsonNumberToDateTimeDataConverter.DATETIME_TYPEINFO.IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            double num;
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    num = (double)(long)reader.Value;
                    break;
                case JsonToken.Float:
                    num = (double)reader.Value;
                    break;
                default:
                    num = 0.0;
                    break;
            }
            return (object)JsonNumberToDateTimeDataConverter.EPOCH_DATETIME.AddSeconds(num);
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
