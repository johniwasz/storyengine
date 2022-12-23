using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models
{


    /// <summary>
    /// This drives which authentication source the system uses. On AWS, it's Cognito. On Azure, it'll be Active Directory.
    /// </summary>
    public enum AuthenticatorType
    {
        Cognito =1
    }
}
