using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class DataSpeechFragmentConverter : JsonStoryConverter
    {
        //static JsonSerializerSettings SpeechFragmentSubclassConversion =
        //    new JsonSerializerSettings() {ContractResolver = new AbstractToConcreteClassConverter<SpeechFragment>()};

        public override bool CanConvert(Type objectType)
        {
            return (typeof(DataSpeechFragment).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<DataSpeechFragment>();

            if (jo.ContainsKey("fragmentType"))
            {

                JToken token = jo["fragmentType"];
                string enumText = token.Value<string>();

                SpeechFragmentType fragType;
                if (Enum.TryParse(enumText, true, out fragType))
                {
                    switch (fragType)
                    {
                        case SpeechFragmentType.AudioFile:
                            return ConvertObject<DataAudioFile, DataSpeechFragment>(jo.ToString(), serializer, conResolver);
                        case SpeechFragmentType.ConditionalFragment:
                            return ConvertObject<DataConditionalFragment, DataSpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.PlainText:
                            return ConvertObject<DataSpeechText, DataSpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.Ssml:
                            return ConvertObject<DataSsmlSpeechFragment, DataSpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.DirectAudioFile:
                            return ConvertObject<DataDirectAudioFile, DataSpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.Break:
                            return ConvertObject<DataSpeechBreakFragment, DataSpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        default:
                            throw new JsonSerializationException(string.Format("Unrecognized SpeechFragmentType {0}",
                                enumText));
                    }
                }
            }
            else
                throw new JsonSerializationException("fragmentType specifier not found");

            throw new JsonSerializationException("Unexpected condition deserializing SpeechFragmentBase");
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
