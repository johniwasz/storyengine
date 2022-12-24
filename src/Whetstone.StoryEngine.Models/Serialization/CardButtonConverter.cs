using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Story.Cards;

namespace Whetstone.StoryEngine.Models.Serialization
{
    class CardButtonConverter : JsonStoryConverter
    {
        //static JsonSerializerSettings SpeechFragmentSubclassConversion =
        //    new JsonSerializerSettings() {ContractResolver = new AbstractToConcreteClassConverter<SpeechFragment>()};

        public override bool CanConvert(Type objectType)
        {
            return (typeof(CardButton).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {

            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<CardButton>();

            if (jo.ContainsKey("cardButtonType"))
            {



                JToken token = jo["cardButtonType"];
                string enumText = token.Value<string>();

                CardButtonType buttonType;
                if (Enum.TryParse(enumText, true, out buttonType))
                {

                    //[MessagePack.Union(0, typeof(SingleNodeMapping))]
                    //    [MessagePack.Union(1, typeof(MultiNodeMapping))]
                    //    [MessagePack.Union(2, typeof(ConditionalNodeMapping))]
                    //    [MessagePack.Union(3, typeof(SlotNodeMapping))]
                    //    [MessagePack.Union(4, typeof(SlotMap))]

                    switch (buttonType)
                    {
                        case CardButtonType.Link:
                            return ConvertObject<CardButtonType, CardButton>(jo.ToString(), serializer,
                                conResolver);

                        default:
                            throw new JsonSerializationException($"Unrecognized CardButtonType {enumText}");
                    }

                }

            }
            else
                throw new JsonSerializationException("nodeMappingType specifier not found");


            throw new JsonSerializationException("Unexpected condition deserializing NodeMappingBase");

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

