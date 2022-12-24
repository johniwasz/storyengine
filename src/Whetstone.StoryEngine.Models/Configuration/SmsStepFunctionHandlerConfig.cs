namespace Whetstone.StoryEngine.Models.Configuration
{
    public class SmsStepFunctionHandlerConfig
    {
        private string _resourceName = null;

        public SmsStepFunctionHandlerConfig()
        {
        }

        public SmsStepFunctionHandlerConfig(string resourceName)
        {
            _resourceName = resourceName;
        }

        /// <summary>
        /// Resource Name - ARN of the resource used to handle sending SMS Messages
        /// </summary>
        /// <remarks>Should be a valid AWS resource name (e.g. arn:aws:states:us-east-1:940085449815:stateMachine:SaveMessageTestFunctions)</remarks>
        public string ResourceName { get { return _resourceName; } set { _resourceName = value; } }

    }


}
