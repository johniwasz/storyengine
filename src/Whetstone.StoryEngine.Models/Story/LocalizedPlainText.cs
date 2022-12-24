using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Whetstone.StoryEngine.Models.Story
{

    [JsonObject()]
    public class LocalizedPlainText : ILocalizedItem
    {

        public LocalizedPlainText()
        {

        }

        public LocalizedPlainText(string text)
        {
            this.Text = text;
        }


        public LocalizedPlainText(string text, string locale)
        {
            this.Text = text;

            this.Locale = locale;

        }

        [JsonProperty("locale", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Locale { get; set; }


        [Required]
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Text { get; set; }

    }

    public static class LocalizedPlainTextExtensions
    {

        public static string GetLocalizedPlainText(this List<LocalizedPlainText> locList, string locale)
        {
            if (locList == null)
                throw new ArgumentNullException(nameof(locList));

            // The locale string can be null. If it is, then pick the default result from the LocalizedPlainText.

            LocalizedPlainText locText = null;

            if (locale != null)
            {
                locText = locList.FirstOrDefault(x => (x.Locale?.Equals(locale, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault(false));
            }

            if (locText == null)
            {
                locText = locList.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Locale));
            }
            string locTextValue = locText?.Text;

            return locTextValue;
        }



    }

}
