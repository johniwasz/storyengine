using System;
using System.Collections.Generic;
using Whetstone.StoryEngine.Models.Story;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class YamlLocalizedResponseSetTypeConverter : IYamlTypeConverter
    {

        private static readonly Type _sequenceEndType = typeof(SequenceEnd);
        private static readonly Type _sequenceStartType = typeof(SequenceStart);

        public bool Accepts(Type type)
        {
            return (type == typeof(LocalizedResponseSet));
        }

        public object ReadYaml(IParser parser, Type type)
        {
            LocalizedResponseSet result = new LocalizedResponseSet();


            var deser = YamlSerializationBuilder.GetYamlDeserializer();

            result.LocalizedResponses = new List<LocalizedResponse>();


            do
            {

                if (parser.Current.GetType() == _sequenceStartType)
                {
                    parser.MoveNext(); // skip the sequence start
                }

                LocalizedResponse locResponse = (LocalizedResponse)deser.Deserialize(parser, typeof(LocalizedResponse));
                result.LocalizedResponses.Add(locResponse);
                parser.MoveNext();

            } while (parser.Current.GetType() != _sequenceEndType);

            return result;



        }


        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
