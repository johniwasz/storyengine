using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// Use to drive the Step Function configuration process for sending notifications.
    /// </summary>
    public class StepFunctionNotificationConfig
    {
        public StepFunctionNotificationConfig()
        {
        }

        public StepFunctionNotificationConfig(string resourceName)
        {
            ResourceName = resourceName;
        }

        /// <summary>
        /// Resource Name - ARN of the resource used to handle processing INotificationRequest messages.
        /// </summary>
        /// <remarks>Should be a valid AWS resource name (e.g. arn:aws:states:us-east-1:940085449815:stateMachine:ProcessNotificationDev)</remarks>
        public string ResourceName { get; set; }

    }


    
}
