using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Whetstone.StoryEngine.Models.Tracking;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class JsonStoryCrumbConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanRead
        {
            get { return true; }
            
        }


        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IStoryCrumb);
        }


        public override void WriteJson(JsonWriter writer,  object value, JsonSerializer serializer)
        {
            JObject jo = new JObject();


           if(value is UniqueItem)
            {
                UniqueItem uniqueItem = value as UniqueItem;
                jo.Add("type", "ui");
                jo.Add("name", uniqueItem.Name);
            }
           else if (value is MultiItem)
            {
                MultiItem multi = value as MultiItem;

                jo.Add("type", "mi");
                jo.Add("name", multi.Name);
                jo.Add("count", multi.Count);
            }
           else if( value is NodeVisitRecord)
            {
                NodeVisitRecord nodeRec = value as NodeVisitRecord;
                jo.Add("type", "nv");
                jo.Add("name", nodeRec.Name);
                jo.Add("count", nodeRec.VisitCount);

            }
            else if(value is SelectedItem)
            {
                SelectedItem selItem = value as SelectedItem;
                jo.Add("type", "si");
                jo.Add("name", selItem.Name);
                jo.Add("value", selItem.Value);
            }
            
           
            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = null;

            try
            {
                jsonObject = JObject.Load(reader);
            }
            catch(Exception ex)
            {
                reader.Skip();

                Console.WriteLine(ex);
                // Ignore error and continue

            }

            IStoryCrumb crumb = default(IStoryCrumb);

            if (jsonObject != null)
            {

                switch (jsonObject["type"].Value<string>())
                {
                    case "ui":
                        UniqueItem uniqueItem = new UniqueItem();
                        uniqueItem.Name = jsonObject["name"].Value<string>();
                        crumb = uniqueItem;
                        break;
                    case "mi":
                        MultiItem multi = new MultiItem();
                        multi.Name = jsonObject["name"].Value<string>();
                        multi.Count = jsonObject["count"].Value<int>();
                        crumb = multi;
                        break;
                    case "nv":
                        NodeVisitRecord nodeRec = new NodeVisitRecord();
                        nodeRec.Name = jsonObject["name"].Value<string>();
                        nodeRec.VisitCount = jsonObject["count"].Value<int>();
                        crumb = nodeRec;
                        break;
                    case "si":
                        SelectedItem selItem = new SelectedItem();
                        selItem.Name = jsonObject["name"].Value<string>();
                        selItem.Value = jsonObject["value"].Value<string>();
                        crumb = selItem;
                        break;

                }
                serializer.Populate(jsonObject.CreateReader(), crumb);
            }

            return crumb;
        }
    }
}
