namespace Whetstone.StoryEngine.Models.Messaging
{

    /// <summary>
    /// Indicates the type of phone number provided by the customer. Used to drive phone number SMS processing.
    /// </summary>
    public enum PhoneTypeEnum
    {
        /// <summary>
        /// Phone type was not validated
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Mobile numbers can receive SMS messages.
        /// </summary>
        Mobile = 1,
        /// <summary>
        /// Phone number is a landline and cannot get SMS messages.
        /// </summary>
        Landline = 2,
        /// <summary>
        /// Voice over IP phone number. Cannot recieve SMS messages.
        /// </summary>
        Voip = 3,
        /// <summary>
        /// Phone number is a valid format, but is otherwise invalid (e.g. invalid area code)
        /// </summary>
        Invalid = 4,
        /// <summary>
        /// Phone verification service validated the number exists, but cannot determine the type of phone number.
        /// </summary>
        Other = 5,
        /// <summary>
        /// Prepaid phone numbers can get sms messages.
        /// </summary>
        Prepay = 6,
        /// <summary>
        /// Out of coverage area phone numbers cannot be resolved. These include 1-800 numbers.
        /// </summary>
        OutOfCoverage = 7,
        /// <summary>
        /// The supplied phone number is does not have a valid format (e.g. less than 10 digits, more than 11 digits)
        /// </summary>
        InvalidFormat = 8
    }
}
