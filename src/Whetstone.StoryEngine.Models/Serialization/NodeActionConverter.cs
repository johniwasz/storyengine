using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class NodeActionConverter : JsonStoryConverter
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

            JObject jo = null;

            jo= JObject.Load(reader);
            
            var conResolver = new AbstractToConcreteClassConverter<NodeActionData>();

            if (jo.ContainsKey("nodeAction"))
            {
                JToken token = jo["nodeAction"];
                string enumText = token.Value<string>();

               NodeActionEnum nodeActionType;
                if (Enum.TryParse(enumText, true, out nodeActionType))
                {
                   

                    switch (nodeActionType)
                    {
                        case NodeActionEnum.NodeVisit:
                            return ConvertObject<NodeVisitRecordActionData, NodeActionData>(jo.ToString(), serializer,
                                conResolver);
                        case NodeActionEnum.Inventory:
                            return ConvertObject<InventoryActionData, NodeActionData>(jo.ToString(), serializer,
                                conResolver);
                        case NodeActionEnum.PhoneMessage:
                            return ConvertObject<PhoneMessageActionData, NodeActionData>(jo.ToString(), serializer,
                               conResolver);
                        case NodeActionEnum.RemoveSelectedItem:
                            return ConvertObject<RemoveSelectedItemActionData, NodeActionData>(jo.ToString(), serializer,
                               conResolver);
                        case NodeActionEnum.ResetState:
                            return ConvertObject<ResetStateActionData, NodeActionData>(jo.ToString(), serializer,
                               conResolver);
                        case NodeActionEnum.SelectedItem:
                            return ConvertObject<RecordSelectedItemActionData, NodeActionData>(jo.ToString(), serializer,
                               conResolver);
                        case NodeActionEnum.AssignValue:
                            return ConvertObject<AssignSlotValueActionData, NodeActionData>(jo.ToString(), serializer,
                               conResolver);
                        case NodeActionEnum.GetPersonalDataAction:
                            return ConvertObject<GetPersonalInfoActionData, NodeActionData>(jo.ToString(), serializer,
                               conResolver);
                        case NodeActionEnum.ValidatePhoneNumber:
                            return ConvertObject<ValidatePhoneNumberActionData, NodeActionData>(jo.ToString(), serializer,
                               conResolver);
                        case NodeActionEnum.SmsConfirmation:
                            return ConvertObject<SmsConfirmationActionData, NodeActionData>(jo.ToString(), serializer,
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
