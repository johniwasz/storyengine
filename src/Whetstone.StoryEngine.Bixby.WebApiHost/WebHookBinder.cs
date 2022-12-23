using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Bixby.Repository.Models;

namespace Whetstone.StoryEngine.Bixby.WebApiHost
{
    public class WebhookBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
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

            BixbyRequest_V1 bixbyRequestModel = JsonConvert.DeserializeObject<BixbyRequest_V1>(modelText);

            bindingContext.Result = ModelBindingResult.Success(bixbyRequestModel);
            return;
        }
    }


}
