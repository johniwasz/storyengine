using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.WebLibrary
{

    public class EnableRequestRewindMiddleware
    {
        private readonly RequestDelegate _next;

        public EnableRequestRewindMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }

    public static class EnableRequestRewindExtension
    {
        public static IApplicationBuilder UseEnableRequestRewind(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnableRequestRewindMiddleware>();
        }
    }
}
