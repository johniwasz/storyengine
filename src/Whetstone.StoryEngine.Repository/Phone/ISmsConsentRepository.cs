using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Repository.Phone
{

    public enum SmsConsentRepositoryType
    {

        DynamoDb = 1,
        Database = 2
    }


    public interface ISmsConsentRepository
    {

        Task<UserPhoneConsent> GetConsentAsync(string consentName, Guid phoneId);

        Task<UserPhoneConsent> GetConsentAsync(Guid consentId);

        Task<UserPhoneConsent> SaveConsentAsync(UserPhoneConsent phoneConsent);
    }
}
