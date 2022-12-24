using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Models.Messaging;
using Xunit;

namespace Whetstone.StoryEngine.Test.DbTests
{

    public class SmsHistoryTest : EntityContextTestBase
    {


        [Fact]
        public async Task UpdateMessageStatus()
        {

            //TODO  Get this from key value settings
            string connection = null;

            DbContextOptions<UserDataContext> contextOptions = GetContextOptions<UserDataContext>(connection);
            ILogger<UserDataContext> contextLogger = CreateLogger<UserDataContext>();
            try
            {
                using (UserDataContext ucx = new UserDataContext(contextOptions, contextLogger))
                {
                    await ucx.LogTwilioSmsCallbackAsync("mesgid", Guid.NewGuid().ToString("N"), false, null, MessageSendStatus.Throttled, "some extended status");
                }

            }
            catch (PostgresException postEx)
            {

                Debug.WriteLine(postEx);

            }
        }

        //[Fact]
        //public async Task AddSmsHistorRecordFunctionAsync()
        //{

        //    OutboundSmsBatchRecord outMessage = StepFunctionLaunchTest.GetSmsOutboundMessage("dev");


        //    Function smsLoggerFunc = new Function();

        //    TestLambdaContext lambdaContext = new TestLambdaContext();

        //   await smsLoggerFunc.FunctionHandler(outMessage, lambdaContext);
        //}



        //[Fact]
        //public async Task AddSmsHistorRecordFunctionBadAsync()
        //{

        //    OutboundSmsBatchRecord outMessage = StepFunctionLaunchTest.GetSmsOutboundMessage("dev");


        //    Function smsLoggerFunc = new Function();

        //    TestLambdaContext lambdaContext = new TestLambdaContext();

        //    PostgresException ex = await Assert.ThrowsAsync<PostgresException>(() => smsLoggerFunc.FunctionHandler(outMessage, lambdaContext));

        //    Assert.Contains("authentication failed", ex.Message);

        //}


        [Fact]
        public void AddSmsHistorRecordDirect()
        {

            OutboundBatchRecord outMessage = StepFunctionLaunchTest.GetSmsOutboundMessage();

            //TODO  Get this from key value settings
            string connection = null;

            DbContextOptions<UserDataContext> contextOptions = GetContextOptions<UserDataContext>(connection);


            //            SELECT t.messageid, t.applicationid, t.userid,
            //   obj->> 'message' as textmessage,
            //   (res->> 'sendTime')::timestamp with time zone as sendTime,
            //   res->> 'extendedStatus' as extendedstatus
            //FROM whetstone.outbound_smsmessage t,
            //   json_array_elements((t.messages::json)) obj,
            //   json_array_elements(obj->'results') res
            //where t.messageid = '2e82608e-e5e8-4681-a1b5-0973f6c3649f';

            ILogger<UserDataContext> contextLogger = CreateLogger<UserDataContext>();

            using (UserDataContext ucx = new UserDataContext(contextOptions, contextLogger))
            {

                ucx.Add<OutboundBatchRecord>(outMessage);

                ucx.SaveChanges();

            }

        }



    }
}
