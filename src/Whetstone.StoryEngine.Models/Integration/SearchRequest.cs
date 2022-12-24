using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Models.Integration
{

    public enum SearchRequestType
    {
        /// <summary>
        /// Executes a standard search.
        /// </summary>
        Search = 0,
        /// <summary>
        /// Determines if the crumbs and related values are supported.
        /// </summary>
        /// <remarks>This is used when processing the CanFulfillIntent request in Alexa.</remarks>
        ValidateCrumbs = 1

    }

    public class SearchRequest
    {

        public List<IStoryCrumb> Crumbs { get; set; }

        public string Locale { get; set; }

        public int? Index { get; set; }

        /// <summary>
        /// Assumes a standard search request if not specified.
        /// </summary>
        public SearchRequestType? RequestType { get; set; }


        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();


            SearchRequestType searchType = RequestType.GetValueOrDefault(SearchRequestType.Search);

            sb.Append(searchType);
            sb.Append("|");

            if (!string.IsNullOrWhiteSpace(Locale))
            {
                sb.Append(Locale);
                sb.Append("|");
            }

            if (Crumbs != null)
            {
                foreach (IStoryCrumb crumb in Crumbs)
                {

                    sb.Append(crumb.ToString());
                    sb.Append("|");
                }
            }


            string searchText = sb.ToString();

            if (searchText.Substring(searchText.Length - 1, 1).Equals("|"))
            {
                searchText = searchText.Substring(0, searchText.Length - 1);

            }

            return searchText;
        }


    }
}
