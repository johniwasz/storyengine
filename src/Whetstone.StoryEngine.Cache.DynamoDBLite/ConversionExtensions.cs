
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Whetstone.StoryEngine.Cache.Models;

namespace Whetstone.StoryEngine.Cache.DynamoDBLite
{
    public static class ConversionExtensions
    {

        internal const string FIELD_HASHKEY = "CacheKey";
        private const string FIELD_CACHEOPTIONS = "CacheOptions";
        internal const string FIELD_TTL = "Ttl";
        internal const string FIELD_VALUE = "Value";

        private const string FIELD_OPT_SPAN = "Span";
        private const string FIELD_OPT_TYPE = "Type";

        //public static AttributeCollection ToDynamoDictionary(this CacheItem cacheItem)
        //{


        //    Dictionary<string, AttributeValue> retAttribs = null;

        //    if (cacheItem != null)
        //    {
        //        retAttribs = new Dictionary<string, AttributeValue>();

        //        if (!string.IsNullOrWhiteSpace(cacheItem.CacheKey))
        //        {
        //            retAttribs.Add(FIELD_HASHKEY, new AttributeValue() { S = cacheItem.CacheKey });


        //            retAttribs.Add(FIELD_TTL, new AttributeValue() { N = cacheItem.Ttl.ToString() });


        //            retAttribs.Add(FIELD_VALUE, new AttributeValue() { B = new MemoryStream(cacheItem.Value) });

        //            if (cacheItem.CacheOptions != null)
        //            {
        //                var cacheOptValues = new AttributeValue();
        //                cacheOptValues.M = new Dictionary<string, AttributeValue>();
        //                cacheOptValues.M.Add(FIELD_OPT_SPAN, new AttributeValue() { N = cacheItem.CacheOptions.Span.ToString() });

        //                if (!string.IsNullOrWhiteSpace(cacheItem.CacheOptions.Type))
        //                    cacheOptValues.M.Add(FIELD_OPT_TYPE, new AttributeValue() { S = cacheItem.CacheOptions.Type });

        //                retAttribs.Add(FIELD_CACHEOPTIONS, cacheOptValues);
        //            }
        //        }
        //    }

        //    return retAttribs;
        //}

        public static CacheItem ToCacheItem(this Amazon.DynamoDb.AttributeCollection attribValues)
        {
            CacheItem cacheItem = null;

            if (attribValues != null)
            {

            
                string hashKey = null;
                if (attribValues.ContainsKey(FIELD_HASHKEY))
                    hashKey = attribValues[FIELD_HASHKEY].ToString();

                if (!string.IsNullOrWhiteSpace(hashKey))
                {
                    cacheItem = new CacheItem { CacheKey = hashKey };

                    if (attribValues.ContainsKey(FIELD_TTL))
                    {
                      
                        cacheItem.Ttl = attribValues[FIELD_TTL].ToInt64();
                    }


                    if (attribValues.ContainsKey(FIELD_VALUE))
                    {
                        cacheItem.Value = attribValues[FIELD_VALUE].ToBinary();

                    }


                    if (attribValues.ContainsKey(FIELD_CACHEOPTIONS))
                    {
                        var cacheOptValues = attribValues[FIELD_CACHEOPTIONS];

                        if (cacheOptValues.Kind == Amazon.DynamoDb.DbValueType.M)
                        {
                            var cacheOptDetails = (Amazon.DynamoDb.AttributeCollection) cacheOptValues.Value;
                            cacheItem.CacheOptions = new CacheOptions();

                            if (cacheOptDetails.ContainsKey(FIELD_OPT_TYPE))
                                cacheItem.CacheOptions.Type = cacheOptDetails[FIELD_OPT_TYPE].ToString();

                            if (cacheOptDetails.ContainsKey(FIELD_OPT_SPAN))
                            {
                                string spanText = cacheOptDetails[FIELD_OPT_SPAN].ToString();
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
