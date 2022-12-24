using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Whetstone.StoryEngine.Models.Twitter
{
    [DebuggerDisplay("Name: {Name}, ScreenName: {ScreenName}")]
    public class TwitterUser
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "default_profile_image")]
        public bool DefaultProfileImage { get; set; }

        [JsonProperty(PropertyName = "profile_background_image_url")]
        public string ProfileBackgroundImageUrl { get; set; }

        [JsonProperty(PropertyName = "profile_background_image_url_https")]
        public string ProfileBackgroundImageUrlHttps { get; set; }

        [JsonProperty(PropertyName = "friends_count")]
        public long FriendsCount { get; set; }

        [JsonProperty(PropertyName = "favourites_count")]
        public long FavoritesCount { get; set; }

        [JsonProperty(PropertyName = "profile_link_color")]
        public long ProfileLinkColor { get; set; }

        [JsonProperty(PropertyName = "utc_offset")]
        public long? UtcOffset { get; set; }

        [JsonProperty(PropertyName = "screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty(PropertyName = "is_translator")]
        public bool? IsTranslator { get; set; }

        [JsonProperty(PropertyName = "followers_count")]
        public long FollowersCount { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        [JsonConverter(typeof(TwitterDateTimeConverter))]
        [JsonProperty(PropertyName = "created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty(PropertyName = "profile_text_color")]
        public long ProfileTextColor { get; set; }

        [JsonProperty(PropertyName = "notifications")]
        public bool? Notifications { get; set; }

        [JsonProperty(PropertyName = "protected")]
        public bool Protected { get; set; }

        [JsonProperty(PropertyName = "statuses_count")]
        public long StatusesCount { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "contributors_enabled")]
        public bool? ContributorsEnabled { get; set; }

        [JsonProperty(PropertyName = "default_profile")]
        public bool IsDefaultProfile { get; set; }

        [JsonProperty(PropertyName = "profile_sidebar_border_color")]
        public long ProfileSidebarBorderColor { get; set; }

        [JsonProperty(PropertyName = "time_zone")]
        public string Timezone { get; set; }

        [JsonProperty(PropertyName = "geo_enabled")]
        public bool? IsGeoEnabled { get; set; }

        [JsonProperty(PropertyName = "verified")]
        public bool IsVerified { get; set; }

        [JsonProperty(PropertyName = "profile_image_url")]
        public string ProfileImageUrl { get; set; }


        [JsonProperty(PropertyName = "profile_image_url_https")]
        public string ProfileImageUrlHttps { get; set; }

        [JsonProperty(PropertyName = "following")]
        public bool? IsFollowing { get; set; }

        [JsonProperty(PropertyName = "profile_background_tile")]
        public bool ProfileBackgroundTile { get; set; }

        [JsonProperty(PropertyName = "listed_count")]
        public long ListedCount { get; set; }


        [JsonProperty(PropertyName = "profile_sidebar_fill_color")]
        public long ProfileSidebarFillColor { get; set; }

        [JsonProperty(PropertyName = "profile_background_color")]
        public long ProfileBackgroundColor { get; set; }

        [JsonProperty(PropertyName = "follow_request_sent")]
        public bool? IsFollowRequestSent { get; set; }

    }
}
