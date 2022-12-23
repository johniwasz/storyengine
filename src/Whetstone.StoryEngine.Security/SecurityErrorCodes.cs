using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Security
{
    public static class SecurityErrorCodes
    {
        // ReSharper disable once InconsistentNaming
        public static readonly string UNEXPECTED_ERROR = "SEC001";

        // ReSharper disable once InconsistentNaming
        public static readonly string USER_NOT_AUTHENTICATED = "SEC002";

        // ReSharper disable once InconsistentNaming
        public static readonly string USER_NOT_CONFIRMED = "SEC003";

        // ReSharper disable once InconsistentNaming
        public static readonly string TOKEN_VALIDATION_FAILED = "SEC004";

        // ReSharper disable once InconsistentNaming
        public static readonly string USER_CONFIRMATION_CODE_EXPIRED = "SEC005";

        // ReSharper disable once InconsistentNaming
        public static string TOKEN_PARSING_FAILED = "SEC006";

        // ReSharper disable once InconsistentNaming
        public static string TOKEN_EXPIRED = "SEC007";


    }
}
