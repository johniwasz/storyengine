using Amazon.Lambda.TestUtilities;
using Whetstone.StoryEngine.WebLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.CoreApi.Tests
{
    public class CoreApiTestBase
    {

        public CoreApiTestBase()
        {
            System.Environment.SetEnvironmentVariable("LOGLEVEL", "Debug");
            System.Environment.SetEnvironmentVariable("LOCALSTOREPATH", @"false");
            System.Environment.SetEnvironmentVariable("ENABLECACHE", @"true");
            //System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"localhost");
            System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"dev-cache-sanjtest.prgrxr.ng.0001.use1.cache.amazonaws.com");
            
            System.Environment.SetEnvironmentVariable("CoreDbConnection", "Host=devsbsstoryengine.c1z3wkpsmw56.us-east-1.rds.amazonaws.com;Database=devsbsstoryengine;Username=sbsadmin;Password=XXXXXXX");
        }



        protected TestLambdaContext GetLambdaContext(bool useLocalStore)
        {
            var context = new TestLambdaContext();

            TestClientContext testContext = new TestClientContext();

            if (useLocalStore)
                System.Environment.SetEnvironmentVariable("LOCALSTOREPATH", @"true");
            //      System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"dev-sbscache.prgrxr.0001.use1.cache.amazonaws.com");

            //System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"localhost");
            context.ClientContext = testContext;
            return context;

        }


    }
}
