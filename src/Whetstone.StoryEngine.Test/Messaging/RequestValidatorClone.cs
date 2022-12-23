// Decompiled with JetBrains decompiler
// Type: Twilio.Security.RequestValidator
// Assembly: Twilio, Version=5.25.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 48597E88-FADE-4005-97B0-7CD5FBFDCAD0
// Assembly location: C:\Users\John\.nuget\packages\twilio\5.25.1\lib\netstandard2.0\Twilio.dll

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Whetstone.StoryEngine.Test.Messaging
{
    /// <summary>Twilio request validator</summary>

    public class RequestValidatorClone
    {
        private readonly HMACSHA1 _hmac;
        private readonly SHA256 _sha;

        /// <summary>Create a new RequestValidator</summary>
        /// <param name="secret">Signing secret</param>
        public RequestValidatorClone(string secret)
        {
            this._hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
            this._sha = SHA256.Create();
        }

        /// <summary>Validate against a request</summary>
        /// <param name="url">Request URL</param>
        /// <param name="parameters">Request parameters</param>
        /// <param name="expected">Expected result</param>
        /// <returns>true if the signature matches the result; false otherwise</returns>
        public bool Validate(string url, NameValueCollection parameters, string expected)
        {
            return this.Validate(url, RequestValidatorClone.ToDictionary(parameters), expected);
        }

        /// <summary>Validate against a request</summary>
        /// <param name="url">Request URL</param>
        /// <param name="parameters">Request parameters</param>
        /// <param name="expected">Expected result</param>
        /// <returns>true if the signature matches the result; false otherwise</returns>
        public bool Validate(string url, IDictionary<string, string> parameters, string expected)
        {
            return RequestValidatorClone.SecureCompare(this.GetValidationSignature(url, parameters), expected);
        }

        public bool Validate(string url, string body, string expected)
        {
            string str1 = new Uri(url).Query.TrimStart('?');
            string expected1 = "";
            char[] chArray1 = new char[1] { '&' };
            foreach (string str2 in str1.Split(chArray1))
            {
                char[] chArray2 = new char[1] { '=' };
                string[] strArray = str2.Split(chArray2);
                if (strArray[0] == "bodySHA256")
                    expected1 = Uri.UnescapeDataString(strArray[1]);
            }
            if (this.Validate(url, (IDictionary<string, string>)new Dictionary<string, string>(), expected))
                return this.ValidateBody(body, expected1);
            return false;
        }

        public bool ValidateBody(string rawBody, string expected)
        {
            return RequestValidatorClone.SecureCompare(Convert.ToBase64String(this._sha.ComputeHash(Encoding.UTF8.GetBytes(rawBody))), expected);
        }

        private static IDictionary<string, string> ToDictionary(NameValueCollection col)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string allKey in col.AllKeys)
                dictionary.Add(allKey, col[allKey]);
            return (IDictionary<string, string>)dictionary;
        }

        public string GetValidationSignature(string url, IDictionary<string, string> parameters)
        {
            StringBuilder stringBuilder = new StringBuilder(url);
            if (parameters != null)
            {
                List<string> stringList = new List<string>((IEnumerable<string>)parameters.Keys);
                StringComparer ordinal = StringComparer.Ordinal;
                stringList.Sort((IComparer<string>)ordinal);
                foreach (string index in stringList)
                    stringBuilder.Append(index).Append(parameters[index] ?? "");
            }
            return Convert.ToBase64String(this._hmac.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString())));
        }

        private static bool SecureCompare(string a, string b)
        {
            if (a == null || b == null)
                return false;
            int length = a.Length;
            if (length != b.Length)
                return false;
            int num = 0;
            for (int index = 0; index < length; ++index)
                num |= (int)a[index] ^ (int)b[index];
            return num == 0;
        }
    }
}