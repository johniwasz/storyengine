
#nullable enable
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class YamlUriTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return (type == typeof(Uri));
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            Scalar scalar = parser.Current as Scalar;

            if (scalar == null)
            {
                throw new Exception("Failed to read Uri from parser.");
            }

            string rawText = scalar.Value;
            parser.MoveNext(); // Advance the parser to avoid issues.  

            return new Uri(rawText);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            Uri? outUri = value as Uri;

            if (outUri != null)
            {
                emitter.Emit(new Scalar(null, null, outUri.ToString(), ScalarStyle.SingleQuoted, true, false));
            }
        }
    }
}
