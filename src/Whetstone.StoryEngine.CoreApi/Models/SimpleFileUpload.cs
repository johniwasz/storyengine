using System;
using Microsoft.AspNetCore.Http;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class SimpleFileUpload
    {
        public IFormFile UploadedFile { get; set; }
        public string ClientId { get; set; }
    }
}
