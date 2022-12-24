using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Whetstone.StoryEngine.Serialization
{
    internal class JsonToMemoryStreamDataConverter : JsonConverter
    {
        private static readonly TypeInfo MEMORYSTREAM_TYPEINFO = typeof(MemoryStream).GetTypeInfo();

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
            return JsonToMemoryStreamDataConverter.MEMORYSTREAM_TYPEINFO.IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            return (object)new MemoryStream(Convert.FromBase64String(reader.Value as string));
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
