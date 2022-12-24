using Amazon;
using System;

namespace Whetstone.StoryEngine.Models.Configuration
{

    public enum DbUserType
    {
        AdminUser = 0,
        StoryEngineUser = 1,
        SessionLoggingUser = 2,
        SmsUser = 3
    }

    /// <summary>
    /// Used to determine how the SMS message is handled before it is sent to the receiver.
    /// </summary>
    public enum SmsHandlerType
    {

        /// <summary>
        /// Dispatches the message directly to the SMS sender (Twilio, SNS, or Pinpoint)
        /// </summary>
        DirectSender = 0,
        /// <summary>
        /// Sends the message to a step function. This is the preferred choice for the StoryEngine.
        /// </summary>
        StepFunctionSender = 2

    }

    public enum SessionLoggerType
    {
        Queue = 1,
        Database = 2
    }

    public class EnvironmentConfig
    {

        public EnvironmentConfig()
        {


        }

        public EnvironmentConfig(RegionEndpoint endpoint, string bucketName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException($"{nameof(bucketName)}");


            Region = endpoint ?? throw new ArgumentNullException($"{nameof(endpoint)}");

            BucketName = bucketName;
        }



        public RegionEndpoint Region { get; set; }

        public string BucketName { get; set; }


        public DbUserType? DbUserType { get; set; }
    }


}
