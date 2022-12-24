using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.CoreApi.Formatters
{
    public class YamlOutputFormatter : TextOutputFormatter
    {

        private readonly ISerializer _yamlSerializer;

        public YamlOutputFormatter()
        {
            _yamlSerializer = YamlSerializationBuilder.GetYamlSerializer();

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/yaml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/yml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/yaml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/yml"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(StoryTitle).IsAssignableFrom(type)
                || typeof(IEnumerable<StoryTitle>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }
            return false;
        }



        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var logger = serviceProvider.GetService(typeof(ILogger<YamlOutputFormatter>)) as ILogger;

            var response = context.HttpContext.Response;

            var buffer = new StringBuilder();
            if (context.Object is IEnumerable<StoryTitle>)
            {
                foreach (StoryTitle story in context.Object as IEnumerable<StoryTitle>)
                {
                    FormatTitle(buffer, story, logger);
                }
            }
            else
            {
                var title = context.Object as StoryTitle;
                FormatTitle(buffer, title, logger);
            }
            return response.WriteAsync(buffer.ToString());
        }

        private void FormatTitle(StringBuilder buffer, StoryTitle title, ILogger logger)
        {
            string titleText = _yamlSerializer.Serialize(title);

            buffer.Append(titleText);

            logger.LogInformation($"Writing {title.Title} to YAML");
        }

    }
}