using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class JsonHttpStatusResult : JsonResult
    {
        private readonly HttpStatusCode? _httpStatus;


        public JsonHttpStatusResult(object data) : base(data)
        {
            Value = data;
        }

        public JsonHttpStatusResult(object data, HttpStatusCode httpStatus) : base(data)
        {
            Value = data;
            _httpStatus = httpStatus;
        }


        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_httpStatus.GetValueOrDefault(HttpStatusCode.OK);

            base.ExecuteResult(context);
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_httpStatus.GetValueOrDefault(HttpStatusCode.OK);

            await base.ExecuteResultAsync(context);
        }

    }
}
