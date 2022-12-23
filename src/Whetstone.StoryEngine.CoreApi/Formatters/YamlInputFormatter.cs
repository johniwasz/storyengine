using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.CoreApi.Formatters
{

    public class YamlInputFormatter : TextInputFormatter
    {

        private readonly IDeserializer _yamlDeser;

        public YamlInputFormatter()
        {
            _yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/yaml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/yml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/yaml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/yml"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }



        protected override bool CanReadType(Type type)
        {
            if (type == typeof(StoryTitle))
            {
                return base.CanReadType(type);
            }
            return false;
        }


        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding effectiveEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (effectiveEncoding == null)
            {
                throw new ArgumentNullException(nameof(effectiveEncoding));
            }

            var request = context.HttpContext.Request;
            string yamlText = null;

            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var logger = serviceProvider.GetService(typeof(ILogger<YamlInputFormatter>)) as ILogger;

            using (var reader = new StreamReader(request.Body, effectiveEncoding))
            {
                try
                {
                    yamlText = await reader.ReadToEndAsync();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error deserializing yaml input");
                    return await InputFormatterResult.FailureAsync();
                }
            }

            StoryTitle title;
            try
            {
                title = _yamlDeser.Deserialize<StoryTitle>(yamlText);

            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error deserializing StoryTitle from yaml input");
                return await InputFormatterResult.FailureAsync();
            }

            return await InputFormatterResult.SuccessAsync(title);
        }

       
    }
}