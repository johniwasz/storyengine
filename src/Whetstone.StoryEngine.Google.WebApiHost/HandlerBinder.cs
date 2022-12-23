using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.Google.Actions.V1;

namespace Whetstone.StoryEngine.Google.WebApiHost
{
    public class HandlerBinder : IModelBinder
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

            HandlerRequest req = HandlerRequest.FromJson(modelText);

            bindingContext.Result = ModelBindingResult.Success(req);
            return;
        }
    }


}
