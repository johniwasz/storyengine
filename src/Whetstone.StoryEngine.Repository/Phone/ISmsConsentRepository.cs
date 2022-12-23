using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Repository.Phone
{

    public enum SmsConsentRepositoryType
    {

        DynamoDb = 1,
        Database =2
    }


    public interface ISmsConsentRepository
    {

        Task<UserPhoneConsent> GetConsentAsync(string consentName, Guid phoneId);

        Task<UserPhoneConsent> GetConsentAsync(Guid consentId);

        Task<UserPhoneConsent> SaveConsentAsync(UserPhoneConsent phoneConsent);
    }
}
