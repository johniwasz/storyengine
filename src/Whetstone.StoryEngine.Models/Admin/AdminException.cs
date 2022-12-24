using Microsoft.Extensions.Logging;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{
    // [JsonConverter(typeof(AdminExceptionConverter))]
    public abstract class AdminException : Exception
    {
        protected AdminException(string publicMessage, string internalMessage, Exception ex) : base(internalMessage, ex)
        {
            this.PublicMessage = publicMessage;

        }

        protected AdminException(string publicMessage, string internalMessage) : base(internalMessage)
        {
            this.PublicMessage = publicMessage;

        }

        public string PublicMessage { get; internal set; }

        public abstract string ErrorCode { get; }

        public abstract string Title { get; }


        public virtual int StatusCode { get; } = 500;

        public virtual LogLevel LogLevel { get; set; } = LogLevel.Error;
    }
}
