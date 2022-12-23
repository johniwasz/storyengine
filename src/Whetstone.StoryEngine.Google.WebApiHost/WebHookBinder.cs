using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Whetstone.StoryEngine.Google.WebApiHost
{
    public class WebhookBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }




            // var json = bindingContext.ActionContext.HttpContext.RequestServices .Formatters.JsonFormatter;

            // Specify a default argument name if none is set by ModelBinderAttribute
            var modelName = bindingContext.BinderModelName;

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
            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            WebhookRequest versionModel = jsonParser.Parse<WebhookRequest>(modelText);

            bindingContext.Result = ModelBindingResult.Success(versionModel);
            return;
        }
    }


}
