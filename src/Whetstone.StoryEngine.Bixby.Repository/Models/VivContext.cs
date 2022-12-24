using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Whetstone.StoryEngine.Bixby.Repository.Models
{
    public class VivContext
    {
        // locale: IETF BCP 47 language tag. Example: en-US.
        [JsonProperty("locale")]
        public string Locale { get; set; }

        // timezone: Time zone as per tz database. Example: America/New_York.
        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        // userId: Unique ID generated based on a given capsule and user.
        [JsonProperty("bixbyUserId")]
        public string BixbyUserId { get; set; }

        // accessToken: OAuth access token (used with Endpoints).
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        // sessionId: Unique identifier for a specific conversation by a specific user. You can pass the sessionID through your external system to track that different requests are from the same conversation by the same user.
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        // testToday: The time in milliseconds that was set by the Simulator.
        [JsonProperty("testToday")]
        public long? TestToday { get; set; }

        // device: The device class ID. Examples include bixby-mobile.
        [JsonProperty("device")]
        public string Device { get; set; }

        // canTypeId: The CAN type ("target") in which the execution is taking place. Example: bixby-mobile-en-US.
        [JsonProperty("canTypeId")]
        public string CanTypeId { get; set; }

        // handsFree: Boolean indicating whether the user is on a hands-free device.
        [JsonProperty("handsFree")]
        public bool HandsFree { get; set; }

        // deviceModel: Boolean indicating whether the user is on a hands-free device.
        [JsonProperty("deviceModel")]
        public string DeviceModel { get; set; }

        // storeCountry: The store country specified by the client, denoted in ISO 3166-1 alpha-2. Example: US or UK.
        [JsonProperty("storeCountry")]
        public string StoreCountry { get; set; }

        // is24HourFormat: Boolean indicating whether the device is set to display time in 24-hour format (true) or AM/PM format (false).
        [JsonProperty("is24HourFormat")]
        public bool Is24HourFormat { get; set; }

        // networkCountry: The country of the device's current network, denoted in ISO 3166-1 alpha-2. Example: US or UK.
        [JsonProperty("networkCountry")]
        public string NetworkCountry { get; set; }

        // clientAppVersion: The version of the app on the client (guessing here)
        [JsonProperty("clientAppVersion")]
        public string ClientAppVersion { get; set; }

        // clientAppId: The Id of the app on the client (guessing here)
        [JsonProperty("clientAppId")]
        public string ClientAppId { get; set; }

    }
}