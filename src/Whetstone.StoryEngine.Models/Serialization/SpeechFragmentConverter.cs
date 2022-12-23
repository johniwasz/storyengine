using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class SpeechFragmentConverter : JsonStoryConverter
    {
        //static JsonSerializerSettings SpeechFragmentSubclassConversion =
        //    new JsonSerializerSettings() {ContractResolver = new AbstractToConcreteClassConverter<SpeechFragment>()};

        public override bool CanConvert(Type objectType)
        {
            return (typeof(SpeechFragment).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<SpeechFragment>();

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
                            return ConvertObject<AudioFile, SpeechFragment>(jo.ToString(), serializer, conResolver);
                        case SpeechFragmentType.ConditionalFragment:
                            return ConvertObject<ConditionalFragment, SpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.PlainText:
                            return ConvertObject<PlainTextSpeechFragment, SpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.Ssml:
                            return ConvertObject<SsmlSpeechFragment, SpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.DirectAudioFile:
                            return ConvertObject<DirectAudioFile, SpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.Break:
                            return ConvertObject<SpeechBreakFragment, SpeechFragment>(jo.ToString(), serializer,
                                conResolver);
                        case SpeechFragmentType.SwitchFragment:
                            return ConvertObject<SwitchConditionFragment, SpeechFragment>(jo.ToString(), serializer,
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