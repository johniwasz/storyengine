// Decompiled with JetBrains decompiler
// Type: Amazon.Lambda.Serialization.Json.JsonToMemoryStreamListDataConverter
// Assembly: Amazon.Lambda.Serialization.Json, Version=1.0.0.0, Culture=neutral, PublicKeyToken=885c28607f98e604
// MVID: E97C5F53-78B2-470D-B0E2-47A1967B09C6
// Assembly location: C:\Users\John\.nuget\packages\amazon.lambda.serialization.json\1.5.0\lib\netstandard2.0\Amazon.Lambda.Serialization.Json.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace Whetstone.StoryEngine.Serialization
{

    /// <summary>
    /// Custom JSON converter for handling special event cases.
    /// </summary>
    internal class JsonToMemoryStreamListDataConverter : JsonConverter
    {
        private static readonly TypeInfo MEMORYSTREAM_LIST_TYPEINFO = typeof(List<MemoryStream>).GetTypeInfo();

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return JsonToMemoryStreamListDataConverter.MEMORYSTREAM_LIST_TYPEINFO.IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            List<MemoryStream> memoryStreamList = new List<MemoryStream>();
            if (reader.TokenType == JsonToken.StartArray)
            {
                do
                {
                    reader.Read();
                    if (reader.TokenType == JsonToken.String)
                    {
                        MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(reader.Value as string));
                        memoryStreamList.Add(memoryStream);
                    }
                }
                while (reader.TokenType != JsonToken.EndArray);
            }
            return (object)memoryStreamList;
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }

}