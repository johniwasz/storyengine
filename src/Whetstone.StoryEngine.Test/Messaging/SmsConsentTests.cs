using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;
using Xunit;

namespace Whetstone.StoryEngine.Test.Messaging
{


    public  class SmsConsentTests : TestServerFixture
    {

        //[Fact]
        //public async Task GetConsentTestAsync()
        //{
        //    string clientUserId = "+12675551212";
        //    string environment = "dev";

        //    IAppMappingReader appReader = Services.GetRequiredService<IAppMappingReader>();
        //    ISmsConsentRepository consentRepo = Services.GetRequiredService<ISmsConsentRepository>();
        //    IStoryUserRepository userRep = Services.GetRequiredService<IStoryUserRepository>();
        //    IPhoneInfoRetriever phoneRetriever = Services.GetService<IPhoneInfoRetriever>();

        //    var dataPhone = await phoneRetriever.GetPhoneInfoAsync(clientUserId);


        //    TitleVersion smsTitle = await appReader.GetTitleAsync( Models.Client.Sms, "+12672140345", "ws02");

        //    StoryRequest storyReq = new StoryRequest();
        //    storyReq.SessionContext = new EngineSessionContext();
        //    storyReq.SessionContext.TitleVersion = smsTitle;
        //    storyReq.Client = Models.Client.Sms;
        //    storyReq.IsRegisteredUser = clientUserId;
        //    var userInfo = await userRep.GetUserAsync(storyReq);

        //    string hashKey = userInfo.GenerateHashKey();


        //    UserPhoneConsent userConsent = await  consentRepo.GetConsentAsync("whetstonesms", dataPhone.Id.Value, hashKey);

        //}


        //[Fact]
        //public async Task SaveConsentTestAsync()
        //{
        //    string clientUserId = "+12675551212";
        //    string environment = "dev";
        //    IAppMappingReader appReader = Services.GetRequiredService<IAppMappingReader>();
        //    IPhoneInfoRetriever phoneRetriever = Services.GetRequiredService<IPhoneInfoRetriever>();
        //    IStoryUserRepository userRep = Services.GetRequiredService<IStoryUserRepository>();


        //    var phoneInfo = await phoneRetriever.GetPhoneInfoAsync( clientUserId);
        //    TitleVersion smsTitle = await appReader.GetTitleAsync( Models.Client.Sms, "+12672140345", "ws02");

        //    StoryRequest storyReq = new StoryRequest();

        //    storyReq.SessionContext = new EngineSessionContext();
        //    storyReq.SessionContext.TitleVersion = smsTitle;
        //    storyReq.Client = Models.Client.Sms;
        //    storyReq.IsRegisteredUser = clientUserId;

        //    var userInfo = await userRep.GetUserAsync(storyReq);


        //    ISmsConsentRepository consentRepo = Services.GetRequiredService<ISmsConsentRepository>();


        //    NotificationSourcePhoneMessageAction notificationTitle = new NotificationSourcePhoneMessageAction();

        //    notificationTitle.ConsentDate = DateTime.UtcNow;
        //    notificationTitle.ConsentBehavior = ConsentBehaviorEnum.RecordConsentGrant;
        //    notificationTitle.ConsentName = "whetstonesms";
        //    notificationTitle.EngineRequestId = Guid.NewGuid();
       
        //    notificationTitle.IsGranted = false;
        //    notificationTitle.TitleVersion = smsTitle;
        //    notificationTitle.TitleUserId = userInfo.Id.Value;


        //    await consentRepo.SaveConsentAsync(notificationTitle, phoneInfo.Id.Value);



        //    //consentRepo.SaveConsentAsync()


        //}



    }
}
