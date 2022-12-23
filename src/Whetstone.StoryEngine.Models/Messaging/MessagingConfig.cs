using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Messaging
{
    public class MessagingConfig
    {



        /// <summary>
        /// Specify how many milliseconds to wait between sending messages. Defaults to 1500 (1.5 seconds if not specified)
        /// </summary>
        /// <remarks>
        /// Messages sent to the same phone number must have at least a one second pause between.
        /// </remarks>
        /// <value>
        /// The message send delay interval. 
        /// </value>
        public int MessageSendDelayInterval { get; set; }


        /// <summary>
        /// Sets the number of times to retry sending a message that returned a throttled error response. If the 
        /// count is exceeded then an error is thrown.
        /// </summary>
        /// <value>
        /// Defaults to three if not specified.
        /// </value>
        public int ThrottleRetryLimit { get; set; }

    }
}
