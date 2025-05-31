using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Whetstone.StoryEngine.Serialization
{
    /// <summary>
    /// Custom ILambdaSerializer implementation which uses Newtonsoft.Json 9.0.1
    /// for serialization.
    /// 
    /// <para>
    /// If the environment variable LAMBDA_NET_SERIALIZER_DEBUG is set to true the JSON coming
    /// in from Lambda and being sent back to Lambda will be logged.
    /// </para>
    /// </summary>
    public class ShallowJsonSerializer : ILambdaSerializer
    {
        private const string DEBUG_ENVIRONMENT_VARIABLE_NAME = "LAMBDA_NET_SERIALIZER_DEBUG";
        private readonly Newtonsoft.Json.JsonSerializer serializer;
        private readonly bool debug;

        /// <summary>Constructs instance of serializer.</summary>
        public ShallowJsonSerializer()
        {
            serializer = Newtonsoft.Json.JsonSerializer.Create(new JsonSerializerSettings()
            {
                ContractResolver = new AwsJsonResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            if (!string.Equals(Environment.GetEnvironmentVariable(DEBUG_ENVIRONMENT_VARIABLE_NAME), "true", StringComparison.OrdinalIgnoreCase))
                return;
            debug = true;
        }

        /// <summary>
        /// Constructs instance of serializer using custom converters.
        /// </summary>
        public ShallowJsonSerializer(IEnumerable<JsonConverter> converters)
          : this()
        {
            if (converters == null)
                return;



            foreach (JsonConverter converter in converters)
                this.serializer.Converters.Add(converter);
        }

        /// <summary>Serializes a particular object to a stream.</summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="response">Object to serialize.</param>
        /// <param name="responseStream">Output stream.</param>
        public void Serialize<T>(T response, Stream responseStream)
        {
            try
            {
                if (this.debug)
                {
                    using (StringWriter stringWriter = new StringWriter())
                    {
                        this.serializer.Serialize((TextWriter)stringWriter, (object)response);
                        Console.WriteLine(string.Format("Lambda Serialize {0}: {1}", (object)response.GetType().FullName, (object)stringWriter.ToString()));
                        using (StreamWriter streamWriter = new StreamWriter(responseStream))
                        {
                            string str = stringWriter.ToString();
                            streamWriter.Write(str);
                            streamWriter.Flush();
                        }
                    }
                }
                else
                {
                    StreamWriter streamWriter = new StreamWriter(responseStream);
                    this.serializer.Serialize((TextWriter)streamWriter, (object)response);
                    streamWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error converting the response object of type {0} from the Lambda function to JSON: {1}", (object)typeof(T).FullName, (object)ex.Message), ex);
            }
        }

        /// <summary>Deserializes a stream to a particular type.</summary>
        /// <typeparam name="T">Type of object to deserialize to.</typeparam>
        /// <param name="requestStream">Stream to serialize.</param>
        /// <returns>Deserialized object from stream.</returns>
        public T Deserialize<T>(Stream requestStream)
        {
            try
            {
                TextReader reader;
                if (this.debug)
                {
                    using (StreamReader streamReader = new StreamReader(requestStream))
                    {
                        string end = streamReader.ReadToEnd();
                        Console.WriteLine(string.Format("Lambda Deserialize {0}: {1}", (object)typeof(T).FullName, (object)end));
                        reader = (TextReader)new StringReader(end);
                    }
                }
                else
                    reader = (TextReader)new StreamReader(requestStream);
                return this.serializer.Deserialize<T>((JsonReader)new JsonTextReader(reader));
            }
            catch (Exception ex)
            {
                Type type = typeof(T);
                throw new Exception(!(type == typeof(string)) ? string.Format("Error converting the Lambda event JSON payload to type {0}: {1}", (object)type.FullName, (object)ex.Message) : string.Format("Error converting the Lambda event JSON payload to a string. JSON strings must be quoted, for example \"Hello World\" in order to be converted to a string: {0}", (object)ex.Message), ex);
            }
        }
    }
}
