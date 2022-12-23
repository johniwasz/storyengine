using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class YamlStringEnumConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {

            Type enumType = GetEnumType(type);

            return (enumType != null) && enumType.IsEnum;


        }

        public object ReadYaml(IParser parser, Type type)
        {
            Scalar parsedEnum = parser.Consume<Scalar>();

            Type enumType = GetEnumType(type);



            Dictionary<string, MemberInfo> serializableValues = enumType.GetMembers()
                .Select(m => new KeyValuePair<string, MemberInfo>(m.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault(), m))
                .Where(pa => !string.IsNullOrEmpty(pa.Key)).ToDictionary(pa => pa.Key, pa => pa.Value, StringComparer.OrdinalIgnoreCase);

            if (!serializableValues.ContainsKey(parsedEnum.Value))
            {
                var members = enumType.GetMembers()
                    .Select(m => new KeyValuePair<string, MemberInfo>(m.Name, m));


                serializableValues = members.Where(pa => !string.IsNullOrEmpty(pa.Key) && pa.Value.MemberType == MemberTypes.Field).ToDictionary(pa => pa.Key, pa => pa.Value, StringComparer.OrdinalIgnoreCase);

                if (!serializableValues.ContainsKey(parsedEnum.Value))
                    throw new YamlException(parsedEnum.Start, parsedEnum.End, $"Value '{parsedEnum.Value}' not found in enum '{type.FullName}'");
            }

            return Enum.Parse(enumType, serializableValues[parsedEnum.Value].Name);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {

            if (value != null)
            {
                Type enumType = GetEnumType(type);

                var enumMember = enumType.GetMember(value.ToString()).FirstOrDefault();
                var yamlValue = enumMember?.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault() ?? value.ToString();
                emitter.Emit(new Scalar(yamlValue));
            }
        }


        private Type GetEnumType(Type nullableEnum)
        {
            if (nullableEnum.IsEnum)
                return nullableEnum;


            Type u = Nullable.GetUnderlyingType(nullableEnum);


            return u;

        }
    }
}
