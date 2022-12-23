using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Actions
{
    public enum NodeActionEnum
    {
        NodeVisit = 0,
        Inventory =1,
        SelectedItem = 2,
        RemoveSelectedItem = 3,
        PhoneMessage = 4,
        ResetState = 5,
        AssignValue = 6,
        GetPersonalDataAction = 7,
        ValidatePhoneNumber = 8,
        SmsConfirmation = 9

    }
}
