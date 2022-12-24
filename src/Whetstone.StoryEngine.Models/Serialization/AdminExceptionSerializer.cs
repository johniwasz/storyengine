namespace Whetstone.StoryEngine.Models.Serialization
{

    /*
    public class AdminExceptionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (typeof(AdminException).IsAssignableFrom(objectType) && !objectType.IsAbstract);
        }


        public override object ReadJson(JsonReader reader, Type objectType,  object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            AdminException adminEx = value as AdminException;

            writer.WriteStartObject();
            if (adminEx != null)
            {
                writer.WritePropertyName("message");
                writer.WriteValue(adminEx.PublicMessage);

            }

            writer.WriteEndObject();
        }
    }
    */
}
