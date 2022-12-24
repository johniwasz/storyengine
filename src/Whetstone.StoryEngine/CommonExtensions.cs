using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine
{
    public static class CommonExtensions
    {


        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        public static string Base64Encode(this string plainText)
        {
            if (plainText == null)
                return null;


            if (plainText == string.Empty)
                return string.Empty;

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            if (base64EncodedData == null)
                return null;

            if (base64EncodedData == string.Empty)
                return string.Empty;

            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static Stream GenerateStreamFromString(this string s)
        {
            var stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(s);
                writer.Flush();
            }
            stream.Position = 0;
            return stream;
        }


        public static DateTime FromUnixTime(this long unixTime)
        {

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);

        }

        public static string JoinList(this IEnumerable<string> strList)
        {

            StringBuilder sb = new StringBuilder();

            if (strList != null)
            {

                foreach (string val in strList)
                {
                    if (val != null)
                        sb.Append(val);
                }
            }

            return sb.ToString();
        }

        public static List<string> CleanTextList(this IEnumerable<TextFragmentBase> baseFragments)
        {
            List<string> simpleTextFrags = new List<string>();

            if (baseFragments != null)
            {
                foreach (TextFragmentBase frag in baseFragments)
                {
                    if (frag is SimpleTextFragment simpleFrag)
                    {
                        string cleanString = simpleFrag.Text;
                        cleanString = String.IsNullOrWhiteSpace(cleanString)
                            ? null
                            : cleanString.Replace("’", @"'")
                                .Replace("“", "\"")
                                .Replace("”", "\"")
                                .Replace("–", "-");
                        if (!string.IsNullOrWhiteSpace(cleanString))
                        {
                            simpleTextFrags.Add(cleanString);
                        }
                    }


                }
            }

            return simpleTextFrags;
        }

        public static string CleanText(this IEnumerable<TextFragmentBase> baseFragments)
        {

            return CleanTextList(baseFragments).JoinList();
        }

    }
}
