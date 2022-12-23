using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Repository.Phone;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Data;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class SmsConsentDatabaseRepository : ISmsConsentRepository
    {
        private readonly ILogger<SmsConsentDatabaseRepository> _logger;

        private IUserContextRetriever _userContextRetriever;

        public SmsConsentDatabaseRepository(IUserContextRetriever userContextRetriever, ILogger<SmsConsentDatabaseRepository> logger)
        {
            _userContextRetriever = userContextRetriever ?? throw new ArgumentNullException($"{nameof(userContextRetriever)}");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }


        public async Task<UserPhoneConsent> GetConsentAsync(Guid consentId)
        {

            if (consentId == default(Guid))
                throw new ArgumentException($"{nameof(consentId)} must be a valid guid");


            UserPhoneConsent phoneConsent = null;

            using (IUserDataContext ucx = await _userContextRetriever.GetUserDataContextAsync())
            {
                phoneConsent = ucx.UserPhoneConsent.Find(consentId);
            }


            return phoneConsent;
        }


        public async Task<UserPhoneConsent> GetConsentAsync(string consentName, Guid phoneId)
        {


            if (string.IsNullOrWhiteSpace(consentName))
                throw new ArgumentException($"{nameof(consentName)} cannot be null or empty");

            if (phoneId == default(Guid))
                throw new ArgumentException($"{nameof(phoneId)} cannot be null or empty");



            UserPhoneConsent phoneConsent = null;

            using (IUserDataContext ucx = await _userContextRetriever.GetUserDataContextAsync())
            {
                var foundConsents = ucx.UserPhoneConsent.Where(x => x.PhoneId.Equals(phoneId)
                                                                    && x.Name.Equals(consentName, StringComparison.OrdinalIgnoreCase))
                                                                    .OrderByDescending(x => x.SmsConsentDate);


                phoneConsent = await foundConsents.FirstOrDefaultAsync();
            }
            return phoneConsent;


        }





        public async Task<UserPhoneConsent> SaveConsentAsync(UserPhoneConsent phoneConsent)
        {

            if (phoneConsent == null)
                throw new ArgumentException($"{nameof(phoneConsent)} cannot be null or empty");

            if (!phoneConsent.SmsConsentDate.HasValue)
                phoneConsent.SmsConsentDate = DateTime.UtcNow;


            try
            {
                using (var userContext = await _userContextRetriever.GetUserDataContextAsync())
                {
                    await userContext.UpsertPhoneConsentAsync(phoneConsent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving consent id {phoneConsent.Id.Value} to database");

                throw;
            }

            return phoneConsent;

        }

    }
}
