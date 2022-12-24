namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// Used to configure default settings that apply to all SMS providers.
    /// </summary>
    public class PhoneConfig
    {


        public PhoneConfig()
        {

        }

        /// <summary>
        /// The default phone number that SMS messages are sent from.
        /// </summary>
        /// <remarks>
        /// This is used to set the default phone number in the OutboundSmsSender. It is read from the SOURCENUMNBER configuration setting.
        /// If the incoming message request does not have a source phone number set, then this value is applied.
        /// </remarks>
        public string SourceSmsNumber { get; set; }

    }
}
