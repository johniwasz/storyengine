using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.CoreApi.ModelBinders
{
    public class LamdaSubmissionBinder<T> : IModelBinder where T : class 
    {

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }




            // var json = bindingContext.ActionContext.HttpContext.RequestServices .Formatters.JsonFormatter;

            // Specify a default argument name if none is set by ModelBinderAttribute

            string modelText = null;
            using (var reader = new StreamReader(bindingContext.HttpContext.Request.Body))
            {
                modelText = await reader.ReadToEndAsync();

                // Do something
            }

            if (string.IsNullOrWhiteSpace(modelText))
            {

                bindingContext.ModelState.AddModelError("nobody", "bodytext is empty");
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }


            // Get the content type
            string contentType = bindingContext.HttpContext.Request.ContentType;

            T versionModel = default;

            if (contentType.Contains("json"))
                versionModel = DeserializeJson<T>(bindingContext, modelText);
            else if(contentType.Contains("yaml") || contentType.Contains("yml"))
                versionModel = DeserializeYaml<T>(bindingContext, modelText);

            bindingContext.Result = ModelBindingResult.Success(versionModel);
            return;
        }

#pragma warning disable CS0693
        private T DeserializeYaml<T>(ModelBindingContext bindingContext, string modelText)
        {
#pragma warning restore CS0693
            var yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();

            T retVal = default;

            try
            {
                retVal = yamlDeser.Deserialize<T>(modelText);
            }
            catch(Exception ex)
            {
                bindingContext.ModelState.AddModelError("yaml deserialization", ex, bindingContext.ModelMetadata);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return retVal;

        }

#pragma warning disable CS0693
        public T DeserializeJson<T>(ModelBindingContext bindingContext, string modelText)
        {
#pragma warning restore CS0693

            var configOptions = (IOptions<MvcNewtonsoftJsonOptions>)
                bindingContext.HttpContext.RequestServices.GetService(typeof(IOptions<MvcNewtonsoftJsonOptions>));


            JsonSerializerSettings serSettings = configOptions.Value.SerializerSettings;

            T retObj = default;

            try
            {
                retObj = JsonConvert.DeserializeObject<T>(modelText, serSettings);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.AddModelError("json deserialization", ex, bindingContext.ModelMetadata);
                bindingContext.Result = ModelBindingResult.Failed();
            }


            return retObj;

        }
    }
}
