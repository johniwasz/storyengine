using System;

namespace Whetstone.StoryEngine.Models
{
    // Minimal models for Azure Blob Session Logger
    public enum Client
    {
        Alexa,
        GoogleAssistant,
        SMS
    }

    public enum StoryRequestType
    {
        Launch,
        Intent,
        SessionEnd
    }

    public class StoryRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ApplicationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public Client Client { get; set; }
        public StoryRequestType RequestType { get; set; }
        public string IntentName { get; set; } = string.Empty;
        public bool IsNewSession { get; set; }
        public string Locale { get; set; } = string.Empty;
    }

    public class StoryResponse
    {
        public OutputSpeech? OutputSpeech { get; set; }
        public bool ShouldEndSession { get; set; }
    }

    public abstract class OutputSpeech
    {
        public abstract string Type { get; }
    }

    public class PlainTextOutputSpeech : OutputSpeech
    {
        public override string Type => "PlainText";
        public string Text { get; set; } = string.Empty;
    }

    public enum CanFulfillIntent
    {
        Yes,
        No,
        Maybe
    }

    public class CanFulfillResponse
    {
        public CanFulfillIntent CanFulfill { get; set; }
    }
}

namespace Whetstone.StoryEngine.Models.Messaging
{
    public class RequestRecordMessage
    {
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string IntentName { get; set; } = string.Empty;
        public DateTime SelectionTime { get; set; }
        public TimeSpan ProcessDuration { get; set; }
        public string Locale { get; set; } = string.Empty;
    }
}

namespace Whetstone.StoryEngine.Data
{
    using Whetstone.StoryEngine.Models;
    using Whetstone.StoryEngine.Models.Messaging;

    public interface ISessionLogger
    {
        Task LogRequestAsync(StoryRequest request, StoryResponse response);
        Task LogRequestAsync(StoryRequest request, StoryResponse response, string rawClientRequestText, string rawClientResponseText);
        Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse);
        Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse, string rawClientRequestText, string rawClientResponseText);
        Task LogRequestAsync(RequestRecordMessage sessionQueueMsg);
    }
}