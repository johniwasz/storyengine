using Newtonsoft.Json;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.Google.Repository.Models
{

    public partial class GoogleLegacyRequest
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("conversation")]
        public Conversation Conversation { get; set; }

        [JsonProperty("requestType")]
        public string RequestType { get; set; }

        [JsonProperty("inputs")]
        public List<Input> Inputs { get; set; }

        [JsonProperty("surface")]
        public Surface Surface { get; set; }

    }

    public partial class Conversation
    {
        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Input
    {
        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("rawInputs")]
        public List<RawInput> RawInputs { get; set; }

        [JsonProperty("arguments")]
        public List<Argument> Arguments { get; set; }
    }

    public partial class Argument
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("boolValue")]
        public bool BoolValue { get; set; }

        [JsonProperty("textValue")]
        public string TextValue { get; set; }
    }

    public partial class RawInput
    {
        [JsonProperty("inputType")]
        public string InputType { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }
    }

    public partial class Surface
    {
        [JsonProperty("capabilities")]
        public List<Capability> Capabilities { get; set; }
    }

    public partial class Capability
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class User
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("profile")]
        public Profile Profile { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }
    }

    public partial class Profile
    {
        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("familyName")]
        public string FamilyName { get; set; }
    }



}

