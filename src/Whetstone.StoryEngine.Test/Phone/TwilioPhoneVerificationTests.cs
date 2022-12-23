using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Npgsql;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository.Phone;
using Xunit;

namespace Whetstone.StoryEngine.Test.Phone
{

    public class TwilioPhoneVerificationTests : TestServerFixture
    {
        [Fact]
        public async Task VerifyPhoneTestAsync()
        {
            
             IPhoneInfoRetriever phoneInfoRetriever = Services.GetRequiredService<IPhoneInfoRetriever>();

             string phoneNumber = "3267304178";
             DataPhone phoneInfo = await   phoneInfoRetriever.GetPhoneInfoAsync(phoneNumber);


            Assert.True(phoneInfo.Type == PhoneTypeEnum.Invalid, $"{phoneNumber} is not invalid");
        }



        [Fact]
        public async Task VerifyBadPhoneTestAsync()
        {

            IPhoneInfoRetriever phoneInfoRetriever = Services.GetRequiredService<IPhoneInfoRetriever>();

            DataPhone phoneInfo;

            //phoneInfo = await phoneInfoRetriever.GetPhoneInfoAsync(ENVIRONMENT, "1111");

            //Debug.Assert(phoneInfo.Type == PhoneTypeEnum.Invalid);

            // AMEX number
            //phoneInfo = await phoneInfoRetriever.GetPhoneInfoAsync(ENVIRONMENT, "18005284800");
            //Debug.Assert(phoneInfo.Type == PhoneTypeEnum.OutOfCoverage);

            // Unclaimed 800 number
            phoneInfo = await phoneInfoRetriever.GetPhoneInfoAsync( "18007957449");
            Assert.True(phoneInfo.Type == PhoneTypeEnum.Voip);

            IUserContextRetriever contextRetriever = Services.GetService<IUserContextRetriever>();


           
            try
            {
                using (var userContext = await contextRetriever.GetUserDataContextAsync())
                {
                    await userContext.UpsertPhoneInfoAsync(phoneInfo);
                }
            }
            catch (NpgsqlException sqlEx)
            {
                Console.WriteLine(sqlEx);
                throw;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

         

        }



        [Fact]
        public async Task GetPhoneInfoAsync()
        {

            IPhoneInfoRetriever phoneInfoRetriever = Services.GetRequiredService<IPhoneInfoRetriever>();

            DataPhone phoneInfo;

            //phoneInfo = await phoneInfoRetriever.GetPhoneInfoAsync(ENVIRONMENT, "1111");

            //Debug.Assert(phoneInfo.Type == PhoneTypeEnum.Invalid);

            // AMEX number
            //phoneInfo = await phoneInfoRetriever.GetPhoneInfoAsync(ENVIRONMENT, "18005284800");
            //Debug.Assert(phoneInfo.Type == PhoneTypeEnum.OutOfCoverage);

            // Unclaimed 800 number
            phoneInfo = await phoneInfoRetriever.GetPhoneInfoAsync("18007957449");
            Assert.True(phoneInfo.Type == PhoneTypeEnum.OutOfCoverage);


        }


    }
}
