using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Whetstone.StoryEngine.Cache.Models;

namespace Whetstone.StoryEngine.Cache.DynamoDB
{
    public static class ConversionExtensions
    {

        internal const string FIELD_HASHKEY = "CacheKey";
        private const string FIELD_CACHEOPTIONS = "CacheOptions";
        internal const string FIELD_TTL = "Ttl";
        internal const string FIELD_VALUE = "Value";

        private const string FIELD_OPT_SPAN = "Span";
        private const string FIELD_OPT_TYPE = "Type";

        public static Dictionary<string, AttributeValue> ToDynamoDictionary(this CacheItem cacheItem)
        {


            Dictionary<string, AttributeValue> retAttribs = null;

            if (cacheItem != null)
            {
                retAttribs = new Dictionary<string, AttributeValue>();

                if (!string.IsNullOrWhiteSpace(cacheItem.CacheKey))
                {
                    retAttribs.Add(FIELD_HASHKEY, new AttributeValue() { S = cacheItem.CacheKey });


                    retAttribs.Add(FIELD_TTL, new AttributeValue() { N = cacheItem.Ttl.ToString() });


                    retAttribs.Add(FIELD_VALUE, new AttributeValue() { B = new MemoryStream(cacheItem.Value) });

                    if (cacheItem.CacheOptions != null)
                    {
                        var cacheOptValues = new AttributeValue();
                        cacheOptValues.M = new Dictionary<string, AttributeValue>();
                        cacheOptValues.M.Add(FIELD_OPT_SPAN, new AttributeValue() { N = cacheItem.CacheOptions.Span.ToString() });

                        if (!string.IsNullOrWhiteSpace(cacheItem.CacheOptions.Type))
                            cacheOptValues.M.Add(FIELD_OPT_TYPE, new AttributeValue() { S = cacheItem.CacheOptions.Type });

                        retAttribs.Add(FIELD_CACHEOPTIONS, cacheOptValues);
                    }
                }
            }

            return retAttribs;
        }

        public static CacheItem ToCacheItem(this Dictionary<string, AttributeValue> attribValues)
        {
            CacheItem cacheItem = null;

            if (attribValues != null)
            {
                string hashKey = null;
                if (attribValues.ContainsKey(FIELD_HASHKEY))
                    hashKey = attribValues[FIELD_HASHKEY].S;

                if (!string.IsNullOrWhiteSpace(hashKey))
                {
                    cacheItem = new CacheItem { CacheKey = hashKey };

                    if (attribValues.ContainsKey(FIELD_TTL))
                    {
                        if (long.TryParse(attribValues[FIELD_TTL].N, out long ttlVal))
                            cacheItem.Ttl = ttlVal;
                    }


                    if (attribValues.ContainsKey(FIELD_VALUE))
                    {
                        using (MemoryStream byteStream = new MemoryStream())
                        {
                            attribValues[FIELD_VALUE].B.CopyTo(byteStream);
                            cacheItem.Value = byteStream.ToArray();
                        }
                    }


                    if (attribValues.ContainsKey(FIELD_CACHEOPTIONS))
                    {
                        var cacheOptValues = attribValues[FIELD_CACHEOPTIONS];

                        if (cacheOptValues.M != null)
                        {
                            var cacheOptDetails = cacheOptValues.M;
                            cacheItem.CacheOptions = new CacheOptions();

                            if (cacheOptDetails.ContainsKey(FIELD_OPT_TYPE))
                                cacheItem.CacheOptions.Type = cacheOptDetails[FIELD_OPT_TYPE].S;

                            if (cacheOptDetails.ContainsKey(FIELD_OPT_SPAN))
                            {
                                string spanText = cacheOptDetails[FIELD_OPT_SPAN].N;
                                if (long.TryParse(spanText, out long spanVal))
                                    cacheItem.CacheOptions.Span = spanVal;
                            }
                        }
                    }
                }
            }


            return cacheItem;
        }
    }
}
