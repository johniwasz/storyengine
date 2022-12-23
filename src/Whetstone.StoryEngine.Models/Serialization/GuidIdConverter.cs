using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class GuidIdConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {

            return (objectType == typeof(Guid?));

          //  return (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Guid));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Guid? retGuid = null;

            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string str = reader.Value.ToString();

                    Guid outGuid;

                    if(Guid.TryParse(str, out outGuid))
                    {
                        retGuid = outGuid;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException(string.Format("Error converting value {0} to type '{1}'.", 
                    reader.Value, 
                    objectType), ex);
            }

            return retGuid;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;
    }
}
