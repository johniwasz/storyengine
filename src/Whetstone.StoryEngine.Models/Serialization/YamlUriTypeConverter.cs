
using System;
using System.Collections.Generic;
using System.Text;
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

        public object ReadYaml(IParser parser, Type type)
        {
            Scalar scalar;

            scalar = parser.Current as Scalar;

            if (scalar == null)
            {
                throw new Exception("Failed to read Uri from parser.");
            }
            else
            {
                string rawText = scalar.Value;
          
                return new Uri(rawText);
            }
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            Uri outUri = value as Uri;

            if(outUri!=null)
            {

                Scalar newQuote = new Scalar(outUri.ToString());

                emitter.Emit(new Scalar(newQuote.Anchor, newQuote.Tag, newQuote.Value, ScalarStyle.SingleQuoted,newQuote.IsPlainImplicit, newQuote.IsQuotedImplicit));

            }
        }
    }
}
