﻿using Amazon.Lambda.TestUtilities;
using Whetstone.StoryEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon;

namespace Whetstone.Queue.SessionLogger.Tests
{
    public class SessionLoggerTestBase
    {

        public SessionLoggerTestBase()
        {

            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);

        }



        protected TestLambdaContext GetLambdaContext()
        {
            var context = new TestLambdaContext();

            TestClientContext testContext = new TestClientContext();


            //System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"localhost");
            context.ClientContext = testContext;
            return context;

        }
    }
}