using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class NodeMapConverter : JsonStoryConverter
    {
        //static JsonSerializerSettings SpeechFragmentSubclassConversion =
        //    new JsonSerializerSettings() {ContractResolver = new AbstractToConcreteClassConverter<SpeechFragment>()};

        public override bool CanConvert(Type objectType)
        {
            return (typeof(NodeMappingBase).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {

            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<NodeMappingBase>();

            if (jo.ContainsKey("nodeMappingType"))
            {



                JToken token = jo["nodeMappingType"];
                string enumText = token.Value<string>();

                NodeMappingType nodeMapType;
                if (Enum.TryParse(enumText, true, out nodeMapType))
                {

                    //[MessagePack.Union(0, typeof(SingleNodeMapping))]
                    //    [MessagePack.Union(1, typeof(MultiNodeMapping))]
                    //    [MessagePack.Union(2, typeof(ConditionalNodeMapping))]
                    //    [MessagePack.Union(3, typeof(SlotNodeMapping))]
                    //    [MessagePack.Union(4, typeof(SlotMap))]

                    switch (nodeMapType)
                    {
                        case NodeMappingType.SingleNodeMapping:
                            return ConvertObject<SingleNodeMapping, NodeMappingBase>(jo.ToString(), serializer,
                                conResolver);
                        case NodeMappingType.MultiNodeMapping:
                            return ConvertObject<MultiNodeMapping, NodeMappingBase>(jo.ToString(), serializer,
                                conResolver);
                        case NodeMappingType.ConditionalNodeMapping:
                            return ConvertObject<ConditionalNodeMapping, NodeMappingBase>(jo.ToString(), serializer,
                                conResolver);
                        case NodeMappingType.SlotNodeMapping:
                            return ConvertObject<SlotNodeMapping, NodeMappingBase>(jo.ToString(), serializer,
                                conResolver);
                        case NodeMappingType.SlotMap:
                            return ConvertObject<SlotMap, NodeMappingBase>(jo.ToString(), serializer,
                                conResolver);

                        default:
                            throw new JsonSerializationException(string.Format("Unrecognized NodeMappingType {0}",
                                enumText));
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
