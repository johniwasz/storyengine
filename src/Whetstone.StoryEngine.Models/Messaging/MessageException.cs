using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models.Messaging
{
    [DataContract]
    public class MessageException : Exception
    {
        public MessageException() : base()
        {
        }


        public MessageException(string message, OutboundBatchRecord outMessage) : base(message)
        {
            this.OutboundMessage = outMessage;
        }


        public MessageException(string message, OutboundBatchRecord outMessage, Exception innerException) : base(message, innerException)
        {
            this.OutboundMessage = outMessage;
        }


        [DataMember]
        [JsonProperty(PropertyName = "outboundMessage")]
        public OutboundBatchRecord OutboundMessage { get; private set; }
    }
}
