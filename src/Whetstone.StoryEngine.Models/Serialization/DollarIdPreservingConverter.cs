using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Whetstone.StoryEngine.Models.Serialization
{

    public class DollarIdPreservingConverter<T> : JsonConverter where T : new()
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IStoryItem).IsAssignableFrom(objectType) && !objectType.IsAbstract;
        }

        public override object ReadJson(JsonReader reader, Type objectType,
                               object existingValue, JsonSerializer serializer)
        {


            JObject jo = JObject.Load(reader);


            T retObj = new T();

            serializer.Populate(jo.CreateReader(), retObj);

            JToken id = jo["$id"];
            if (id != null)
            {
                string idText = id.ToString();

                IStoryItem storyItem = (IStoryItem)retObj;
                Guid idGuid = default(Guid);

                if (Guid.TryParse(idText, out idGuid))
                {
                    storyItem.UniqueId = idGuid;

                    serializer.ReferenceResolver.AddReference(serializer.Context, idGuid.ToString(), retObj);
                }



            }


            return retObj;
        }

        public override void WriteJson(JsonWriter writer, object value,
                                       JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }


        public override bool CanRead => true;

        public override bool CanWrite => false;
    }
}
