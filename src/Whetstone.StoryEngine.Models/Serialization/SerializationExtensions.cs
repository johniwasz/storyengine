using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public static class SerializationExtensions
    {

        public static string GetDescriptionFromEnumValue(this Enum value)
        {
            var customAttribs = typeof(EnumMemberAttribute).GetTypeInfo().GetCustomAttributes();

            EnumMemberAttribute attrib = customAttribs.SingleOrDefault() as EnumMemberAttribute;

            return attrib == null ? value.ToString() : attrib.Value;
        }

    }
}
