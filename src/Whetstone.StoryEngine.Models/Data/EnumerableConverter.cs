using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.Models.Data
{
    public class EnumerableConverter : JsonConverter
    {

        public EnumerableConverter()
        {


        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            if (value != null)
            {
                JToken t = JToken.FromObject(value);
                t.WriteTo(writer);
            }
            //JToken t = JToken.FromObject(value);

            //if (t.Type != JTokenType.Object)
            //{
            //    t.WriteTo(writer);
            //}
            //else
            //{
            //    JObject o = (JObject)t;
            //    IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

            //    o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

            //    o.WriteTo(writer);
            //}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IEnumerable<>);
        }
    }
}
