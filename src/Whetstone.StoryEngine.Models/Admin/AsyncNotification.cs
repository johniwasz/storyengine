﻿using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AsyncNotification
    {
        [JsonProperty(PropertyName = "notificationId")]
        public string NotificationId { get; set; }
    }
}
