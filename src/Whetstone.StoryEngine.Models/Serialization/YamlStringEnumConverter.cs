#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Serialization
{
    /// <summary>
    /// A YamlDotNet type converter that serializes and deserializes enums as strings,
    /// using the EnumMemberAttribute value if present, or the enum name otherwise.
    /// </summary>
    public class YamlStringEnumConverter : IYamlTypeConverter
    {
        /// <summary>
        /// Determines if the converter can handle the specified type (enum or nullable enum).
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an enum or nullable enum; otherwise, false.</returns>
        public bool Accepts(Type type)
        {
            Type enumType = GetEnumType(type);
            return (enumType != null) && enumType.IsEnum;
        }

        /// <summary>
        /// Reads a YAML scalar and converts it to the corresponding enum value.
        /// Supports both EnumMemberAttribute values and enum field names.
        /// </summary>
        /// <param name="parser">The YAML parser.</param>
        /// <param name="type">The target enum type.</param>
        /// <param name="rootDeserializer">The root deserializer (not used in this implementation).</param>
        /// <returns>The parsed enum value.</returns>
        /// <exception cref="YamlException">Thrown if the value cannot be mapped to the enum.</exception>
        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            Scalar parsedEnum = parser.Consume<Scalar>();
            Type enumType = GetEnumType(type);

            // Try to map using EnumMemberAttribute values
            Dictionary<string, MemberInfo> serializableValues = enumType.GetMembers()
                .Select(m => new KeyValuePair<string, MemberInfo>(m.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault(), m))
                .Where(pa => !string.IsNullOrEmpty(pa.Key)).ToDictionary(pa => pa.Key, pa => pa.Value, StringComparer.OrdinalIgnoreCase);

            if (!serializableValues.ContainsKey(parsedEnum.Value))
            {
                // Fallback: map using enum field names
                var members = enumType.GetMembers()
                    .Select(m => new KeyValuePair<string, MemberInfo>(m.Name, m));

                serializableValues = members.Where(pa => !string.IsNullOrEmpty(pa.Key) && pa.Value.MemberType == MemberTypes.Field).ToDictionary(pa => pa.Key, pa => pa.Value, StringComparer.OrdinalIgnoreCase);

                if (!serializableValues.ContainsKey(parsedEnum.Value))
                    throw new YamlException(parsedEnum.Start, parsedEnum.End, $"Value '{parsedEnum.Value}' not found in enum '{type.FullName}'");
            }

            return Enum.Parse(enumType, serializableValues[parsedEnum.Value].Name);
        }

        /// <summary>
        /// Writes an enum value as a YAML scalar, using the EnumMemberAttribute value if present,
        /// or the enum name otherwise.
        /// </summary>
        /// <param name="emitter">The YAML emitter.</param>
        /// <param name="value">The enum value to write.</param>
        /// <param name="type">The enum type.</param>
        /// <param name="serializer">The serializer (not used in this implementation).</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a valid YAML value cannot be determined.</exception>
        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null when writing YAML.");

            Type enumType = GetEnumType(type);

            // Convert the value to string and ensure it's not null
            string enumValue = value.ToString() ?? throw new ArgumentNullException(nameof(value), "Value cannot be null when writing YAML.");

            // Try to get the EnumMemberAttribute value
            var enumMember = enumType.GetMember(enumValue).FirstOrDefault();
            var yamlValue = enumMember?.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault() ?? value.ToString();
            if (yamlValue == null)
            {
                throw new InvalidOperationException($"Unable to determine a valid YAML value for the enum member '{value}' of type '{type.FullName}'.");
            }
            emitter.Emit(new Scalar(yamlValue));
        }

        /// <summary>
        /// Gets the underlying enum type, handling nullable enums.
        /// </summary>
        /// <param name="nullableEnum">The type to check.</param>
        /// <returns>The enum type.</returns>
        /// <exception cref="ArgumentException">Thrown if the type is not an enum or nullable enum.</exception>
        private Type GetEnumType(Type nullableEnum)
        {
            if (nullableEnum.IsEnum)
                return nullableEnum;

            Type? underlyingType = Nullable.GetUnderlyingType(nullableEnum);
            if (underlyingType == null)
                throw new ArgumentException($"The provided type '{nullableEnum.FullName}' is not a nullable enum or an enum.", nameof(nullableEnum));

            return underlyingType;
        }
    }
}
