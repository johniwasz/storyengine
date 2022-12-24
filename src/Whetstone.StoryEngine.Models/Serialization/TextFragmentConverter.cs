using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class TextFragmentConverter : JsonStoryConverter
    {
        static JsonSerializerSettings TextFragmentSubclassConversion = new JsonSerializerSettings() { ContractResolver = new AbstractToConcreteClassConverter<TextFragmentBase>() };

        public override bool CanConvert(Type objectType)
        {
            return (typeof(TextFragmentBase).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<TextFragmentBase>();

            if (jo.ContainsKey("fragmentType"))
            {

                JToken token = jo["fragmentType"];
                string enumText = token.Value<string>();

                TextFragmentType fragType;
                if (Enum.TryParse(enumText, true, out fragType))
                {
                    switch (fragType)
                    {
                        case TextFragmentType.Simple:
                            return ConvertObject<SimpleTextFragment, TextFragmentBase>(jo.ToString(), serializer, conResolver);
                        case TextFragmentType.Conditional:
                            return ConvertObject<ConditionalTextFragment, TextFragmentBase>(jo.ToString(), serializer, conResolver);
                        default:
                            throw new JsonSerializationException(string.Format("Unrecognized TextFragmentType {0}", enumText));
                    }

                }
            }
            else
                throw new JsonSerializationException("fragmentType specifier not found");

            throw new JsonSerializationException("Unexpected condition deserializing TextFragmentBase");
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
