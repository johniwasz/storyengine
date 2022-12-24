using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class ItemTypeConverter : JsonStoryConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return (typeof(InventoryItemBase).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }

        public override bool CanRead
        {
            get { return true; }

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            JObject jo = JObject.Load(reader);

            var conResolver = new AbstractToConcreteClassConverter<InventoryItemBase>();

            if (jo.ContainsKey("itemType"))
            {
                JToken token = jo["itemType"];
                string enumText = token.Value<string>();

                InventoryItemType itemType;
                if (Enum.TryParse(enumText, true, out itemType))
                {


                    switch (itemType)
                    {

                        case InventoryItemType.Unique:
                            return ConvertObject<UniqueItem, InventoryItemBase>(jo.ToString(), serializer,
                                conResolver);

                        case InventoryItemType.Multi:
                            return ConvertObject<MultiItem, InventoryItemBase>(jo.ToString(), serializer,
                                conResolver);


                        default:
                            throw new JsonSerializationException(string.Format("Unrecognized ItemType {0}",
                                enumText));
                    }

                }

            }
            else
                throw new JsonSerializationException("itemType specifier not found");

            throw new JsonSerializationException("Unexpected condition deserializing InventoryItemBase");

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
