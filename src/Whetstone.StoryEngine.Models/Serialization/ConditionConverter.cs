using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Conditions;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class NodeConditionConverter : JsonStoryConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return (typeof(NodeActionData).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<StoryConditionBase>();

            if (jo.ContainsKey("conditionType"))
            {
                JToken token = jo["conditionType"];
                string enumText = token.Value<string>();

                ConditionType nodeActionType;
                if (Enum.TryParse(enumText, true, out nodeActionType))
                {


                    switch (nodeActionType)
                    {
                        case ConditionType.Inventory:
                            return ConvertObject<InventoryCondition, StoryConditionBase>(jo.ToString(), serializer,
                                conResolver);
                        case ConditionType.NodeVisit:
                            return ConvertObject<NodeVisitCondition, StoryConditionBase>(jo.ToString(), serializer,
                                conResolver);
                        case ConditionType.UserClientCondition:
                            return ConvertObject<UserClientCondition, StoryConditionBase>(jo.ToString(), serializer,
                                conResolver);
                        default:
                            throw new JsonSerializationException(string.Format("Unrecognized ConditionType {0}",
                                enumText));
                    }

                }

            }
            else
                throw new JsonSerializationException("conditionType specifier not found");




            throw new JsonSerializationException("Unexpected condition deserializing StoryConditionBase");

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
