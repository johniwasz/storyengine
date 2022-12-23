using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PhoneNumbers;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine
{
    public class PhoneUtility
    {
        public static readonly string TwilioService = "Twilio";

        public static readonly string Twilio_Error_UnprovisionedOrOutOfCoverage = "60600";


        private static readonly Lazy<PhoneNumberUtil> phoneUtilLoader = new Lazy<PhoneNumberUtil>(() => PhoneNumberUtil.GetInstance());

#pragma warning disable IDE0060 // Remove unused parameter
        public static (string FormattedNumber, bool IsValid) ValidateFormat(string phoneNumber, string userLocale)
#pragma warning restore IDE0060 // Remove unused parameter
        {

            // string pattern = @"((1?)|(1-/?))((?:\(?[2-9](?(?=1)1[02-9]|(?(?=0)0[1-9]|\d{2}))\)?\D{0,3})(?:\(?[2-9](?(?=1)1[02-9]|\d{2})\)?\D{0,3})\d{4})";

            string pattern = @"(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:\(\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9])\s*\)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)?([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})$";
            Regex checker = new Regex(pattern);


            if(string.IsNullOrWhiteSpace(phoneNumber))
                return (null, false);

            bool isValidNumber = checker.IsMatch(phoneNumber);
            string formattedNumber = null;
            if (isValidNumber)
            {
                // Get all digits from string.
                string phoneDigits = new string(phoneNumber.Where(Char.IsDigit).ToArray());

                if (phoneDigits.Length == 11)
                {
                    // If the phone number is 11 digits and the first number is not 1
                    // then the number passed the regular expression check, but is still
                    // not a valid phone number
                    if (phoneDigits[0] != '1')
                        isValidNumber = false;
                    else
                        formattedNumber = $"+{phoneDigits}";

                }
                else if (phoneDigits.Length == 10)
                {
                    formattedNumber = $"+1{phoneDigits}";
                }
                else
                {
                    // Even if the format passes the regular expression, but is not 10 or 11 digits, 
                    // then fail the format.
                    isValidNumber = false;

                }
            }
            
            return (formattedNumber, isValidNumber);
        }



        public static bool PhoneSupportsSms(PhoneTypeEnum phoneType)
        {
            return (phoneType == PhoneTypeEnum.Mobile || phoneType == PhoneTypeEnum.Prepay);
        }


        public static string FormatE164(string phoneNumber)
        {

            var phoneUtil = phoneUtilLoader.Value;

            PhoneNumber phoneNum = phoneUtil.Parse(phoneNumber, "US");

            string formattedNumber = phoneUtil.Format(phoneNum, PhoneNumberFormat.E164);

            return formattedNumber;
        }

        public static string FormatPhoneNumberUS(string phoneNumber)
        {
            string phoneDigits = new string(phoneNumber.Where(Char.IsDigit).ToArray());
            string returnNumber = null;
            if (phoneDigits.Length == 11)
            {
                // Format to #-###-###-####
                StringBuilder phoneBuilder = new StringBuilder();
                phoneBuilder.Append(phoneDigits.Substring(0, 1));
                phoneBuilder.Append("-");
                phoneBuilder.Append(phoneDigits.Substring(1, 3));
                phoneBuilder.Append("-");
                phoneBuilder.Append(phoneDigits.Substring(4, 3));
                phoneBuilder.Append("-");
                phoneBuilder.Append(phoneDigits.Substring(7, 4));
                returnNumber = phoneBuilder.ToString();

            }
            else if (phoneDigits.Length == 10)
            {
                // Format to ###-###-####
                StringBuilder phoneBuilder = new StringBuilder();
                phoneBuilder.Append(phoneDigits.Substring(0, 3));
                phoneBuilder.Append("-");
                phoneBuilder.Append(phoneDigits.Substring(3, 3));
                phoneBuilder.Append("-");
                phoneBuilder.Append(phoneDigits.Substring(6, 4));
                returnNumber = phoneBuilder.ToString();
            }


            return returnNumber;
        }

        public static string FormatPhoneNumberWithSpacesUS(string phoneNumber)
        {
            //
            // Bixby doesn't support SSML or any fancy ways of saying a phone number so to have a number
            // repeated back using individual digits, we insert spaces between the numbers.
            //
            string phoneDigits = new string(phoneNumber.Where(Char.IsDigit).ToArray());
            string returnNumber = null;

            if ( phoneDigits.Length > 0 )
            {
                StringBuilder phoneBuilder = new StringBuilder();

                for (int n = 0; n < phoneDigits.Length; n++)
                {
                    if (n != 0)
                    {
                        phoneBuilder.Append(" ");
                    }

                    phoneBuilder.Append(phoneDigits[n]);
                }

                returnNumber = phoneBuilder.ToString();

            }

            return returnNumber;
        }
    }
}
