using System.Collections.Generic;

namespace Whetstone.StoryEngine.LambdaUtilities.Models
{
    internal class RequestProcessResponse
    {
        /// <summary>
        /// Returns whether or not the request succeeded
        /// </summary>
        internal bool IsProcessed { get; set; }



        /// <summary>
        /// If IsProcessed is false, then this text is surfaced in the CloudFormation template status.
        /// </summary>
        internal string TemplateStatus { get; set; }


        /// <summary>
        /// List of name value pairs to send in the response.
        /// </summary>
        internal Dictionary<string, string> Data { get; set; }

    }
}
