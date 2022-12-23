using Microsoft.Extensions.Logging;
using System;

namespace Whetstone.StoryEngine
{
    public static class ErrorEvents
    {

        public static readonly EventId SessionAuditError = new EventId(1, "SessionAuditError");

        public static readonly EventId LambdaFunctionConfigError = new EventId(2, "LambdaFunctionConfigError");

        public static readonly EventId ProcessRequestError = new EventId(3, "ProcessRequestError");

        public static readonly EventId ContainerConfigLoadError = new EventId(4, "ContainerConfigLoadError");

        public static readonly EventId TitleMappingError = new EventId(5, "TitleMappingError");

        public static readonly EventId StoryRequestProcessError = new EventId(6, "StoryRequestProcessError");

        public static readonly EventId UserRetrievalError = new EventId(7, "UserRetrievalError");

        public static readonly EventId InboundSmsProcessingError = new EventId(8, "InbooundSmsProcessingError");

        public static readonly EventId ClientRequestParsingError = new EventId(9, "ClientRequestParsingError");
    }
}
