// Decompiled with JetBrains decompiler
// Type: Amazon.Lambda.Serialization.Json.AwsResolver
// Assembly: Amazon.Lambda.Serialization.Json, Version=1.0.0.0, Culture=neutral, PublicKeyToken=885c28607f98e604
// MVID: E97C5F53-78B2-470D-B0E2-47A1967B09C6
// Assembly location: C:\Users\John\.nuget\packages\amazon.lambda.serialization.json\1.5.0\lib\netstandard2.0\Amazon.Lambda.Serialization.Json.dll

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Whetstone.StoryEngine.Serialization
{
    /// <summary>
    /// Custom contract resolver for handling special event cases.
    /// </summary>
    internal class AwsJsonResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            if (type.FullName.Equals("Amazon.S3.Util.S3EventNotification+ResponseElementsEntity", StringComparison.Ordinal))
            {
                foreach (JsonProperty jsonProperty in (IEnumerable<JsonProperty>)properties)
                {
                    if (jsonProperty.PropertyName.Equals("XAmzRequestId", StringComparison.Ordinal))
                        jsonProperty.PropertyName = "x-amz-request-id";
                    else if (jsonProperty.PropertyName.Equals("XAmzId2", StringComparison.Ordinal))
                        jsonProperty.PropertyName = "x-amz-id-2";
                }
            }
            else if (type.FullName.Equals("Amazon.Lambda.KinesisEvents.KinesisEvent+Record", StringComparison.Ordinal))
            {
                foreach (JsonProperty jsonProperty in (IEnumerable<JsonProperty>)properties)
                {
                    if (jsonProperty.PropertyName.Equals("Data", StringComparison.Ordinal))
                        jsonProperty.Converter = (JsonConverter)new JsonToMemoryStreamDataConverter();
                    else if (jsonProperty.PropertyName.Equals("ApproximateArrivalTimestamp", StringComparison.Ordinal))
                        jsonProperty.Converter = (JsonConverter)new JsonNumberToDateTimeDataConverter();
                }
            }
            else if (type.FullName.Equals("Amazon.DynamoDBv2.Model.StreamRecord", StringComparison.Ordinal))
            {
                foreach (JsonProperty jsonProperty in (IEnumerable<JsonProperty>)properties)
                {
                    if (jsonProperty.PropertyName.Equals("ApproximateCreationDateTime", StringComparison.Ordinal))
                        jsonProperty.Converter = (JsonConverter)new JsonNumberToDateTimeDataConverter();
                }
            }
            else if (type.FullName.Equals("Amazon.DynamoDBv2.Model.AttributeValue", StringComparison.Ordinal))
            {
                foreach (JsonProperty jsonProperty in (IEnumerable<JsonProperty>)properties)
                {
                    if (jsonProperty.PropertyName.Equals("B", StringComparison.Ordinal))
                        jsonProperty.Converter = (JsonConverter)new JsonToMemoryStreamDataConverter();
                    else if (jsonProperty.PropertyName.Equals("BS", StringComparison.Ordinal))
                        jsonProperty.Converter = (JsonConverter)new JsonToMemoryStreamListDataConverter();
                }
            }
            else if (type.FullName.Equals("Amazon.Lambda.SQSEvents.SQSEvent+MessageAttribute", StringComparison.Ordinal))
            {
                foreach (JsonProperty jsonProperty in (IEnumerable<JsonProperty>)properties)
                {
                    if (jsonProperty.PropertyName.Equals("BinaryValue", StringComparison.Ordinal))
                        jsonProperty.Converter = (JsonConverter)new JsonToMemoryStreamDataConverter();
                    else if (jsonProperty.PropertyName.Equals("BinaryListValues", StringComparison.Ordinal))
                        jsonProperty.Converter = (JsonConverter)new JsonToMemoryStreamListDataConverter();
                }
            }
            else if (type.FullName.StartsWith("Amazon.Lambda.CloudWatchEvents."))
            {
                Type baseType = type.GetTypeInfo().BaseType;
                bool? nullable1;
                if (baseType is null)
                {
                    nullable1 = new bool?();
                }
                else
                {
                    string fullName = baseType.FullName;
                    if (fullName == null)
                    {
                        nullable1 = new bool?();
                    }
                    else
                    {
                        string str = "Amazon.Lambda.CloudWatchEvents.CloudWatchEvent`";
                        int num = 4;
                        nullable1 = new bool?(fullName.StartsWith(str, (StringComparison)num));
                    }
                }
                bool? nullable2 = nullable1;
                if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                {
                    foreach (JsonProperty jsonProperty in (IEnumerable<JsonProperty>)properties)
                    {
                        if (jsonProperty.PropertyName.Equals("DetailType", StringComparison.Ordinal))
                            jsonProperty.PropertyName = "detail-type";
                    }
                }
            }
            return properties;
        }
    }
}
