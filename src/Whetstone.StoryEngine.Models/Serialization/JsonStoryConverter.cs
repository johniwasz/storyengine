using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public abstract class JsonStoryConverter : JsonConverter
    {




        protected T ConvertObject<T, TC>(string text, JsonSerializer serializer, AbstractToConcreteClassConverter<TC> concreteResolver)
        {
            T retVal = default(T);

            lock (serializer)
            {
                //Hold the original value(s) to reset later
                var originalContractResolver = serializer.ContractResolver;
                //Set custom value(s)
                serializer.ContractResolver = concreteResolver;
                using (StringReader sr = new StringReader(text))
                {
                   retVal = (T) serializer.Deserialize(sr, typeof(T));

                }
                   
   
                ////Serialization with custom properties
                //retVal = JsonConvert.DeserializeObject<T>(text,
                //        SpeechFragmentSubclassConversion);
                //Reset original value(s)
                serializer.ContractResolver = originalContractResolver;
            }

            return retVal;
        }
    }
}
