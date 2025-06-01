using Amazon;
using Amazon.S3;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Repository;
using Xunit;

namespace Whetstone.StoryEngine.Cache.DynamoDB.Test
{
    public class DynamoCacheTest
    {

        [Fact]
        public async Task GetUserDynamoTestAsync()
        {

            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;

            List<Guid> userIds = new List<Guid>();
            for (int i = 0; i < 100; i++)
            {
                userIds.Add(Guid.NewGuid());

            }

            WriteThreadLog();


            var distCacheDict = DynamoTestUtil.GetDynamoDbCache();

            var memCache = DynamoTestUtil.GetInMemoryCache();

            ILogger<TitleCacheRepository> titleCacheLogger = DynamoTestUtil.GetLogger<TitleCacheRepository>();

            ILogger<S3FileReader> s3FileStore = DynamoTestUtil.GetLogger<S3FileReader>();

            IAmazonS3 s3Client = DynamoTestUtil.GetAmazonS3Client();

            EnvironmentConfig envConfig = new EnvironmentConfig
            {
                BucketName = "whetstonebucket-dev-s3bucket-1nridm382p5vm",
                Region = RegionEndpoint.USEast1
            };

            IOptions<EnvironmentConfig> envConfigOpts = Options.Create<EnvironmentConfig>(envConfig);

            IFileReader fileRep = new S3FileReader(envConfigOpts, s3Client, s3FileStore);

            ITitleCacheRepository titleCacheRep = new TitleCacheRepository(fileRep, distCacheDict, memCache, titleCacheLogger);

            IAppMappingReader appReader = new CacheAppMappingReader(titleCacheRep);

            IStoryUserRepository userRep = DynamoTestUtil.GetStoryUserRepository();

            ILogger<SessionStoreCacheManager> sessionLogger = DynamoTestUtil.GetLogger<SessionStoreCacheManager>();

            ISessionStoreManager storManager = new SessionStoreCacheManager(distCacheDict, sessionLogger);

            var bag = new ConcurrentBag<TimingResponse>();
            var tasks = userIds.Select(async item =>
            {
                TimingResponse response = new TimingResponse();
                Stopwatch timer = new Stopwatch();
                timer.Start();
                try
                {
                    Client curClient = Client.Alexa;
                    string clientAppId = "amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0";

                    //Stopwatch titleTime = Stopwatch.StartNew();
                    TitleVersion titleVer = await appReader.GetTitleAsync(curClient, clientAppId, null);
                    //Debug.WriteLine($"TitleRetrieval Time: {titleTime.ElapsedMilliseconds}");

                    StoryRequest storyReq = new StoryRequest();

                    storyReq.Client = curClient;
                    storyReq.ApplicationId = clientAppId;

                    storyReq.IsNewSession = true;
                    storyReq.RequestType = StoryRequestType.Launch;
                    storyReq.RequestTime = DateTime.UtcNow;
                    storyReq.SessionId = Guid.NewGuid().ToString("N");
                    storyReq.UserId = item.ToString("N");
                    storyReq.Locale = "en-US";
                    storyReq.SessionContext = new EngineSessionContext();
                    storyReq.SessionContext.TitleVersion = titleVer;

                    DataTitleClientUser user = await userRep.GetUserAsync(storyReq);
                    user.CurrentNodeName = "BeginNode";


                    await storManager.SaveSessionStartTypeAsync(storyReq);

                    //// Now save the user.

                    user.TitleState = new List<IStoryCrumb>() { new SelectedItem() { Name = "someitem", Value = "somevalue" } };

                    await userRep.SaveUserAsync(user);

                    SessionStartType? startType = await storManager.GetSessionStartTypeAsync(storyReq);


                    Console.WriteLine($"User {user.UserId}");
                }
                catch (Exception ex)
                {
                    response.Exceptions.Add(ex);
                }
                finally
                {
                    timer.Stop();
                    response.Milliseconds = timer.ElapsedMilliseconds;
                    bag.Add(response);

                }


            });
            await Task.WhenAll(tasks);
            var count = bag.Count;
            var averageTime = bag.Select(x => x.Milliseconds).Average();

            Console.WriteLine($"average time {averageTime}");

            var overTimes = bag.Where(x => x.Milliseconds > 1000 || (x.Exceptions?.Any()).GetValueOrDefault(false));
            foreach (var overTime in overTimes)
            {
                Console.WriteLine(overTime.Milliseconds);

                if ((overTime.Exceptions?.Any()).GetValueOrDefault(false))
                {
                    foreach (Exception ex in overTime.Exceptions)
                    {
                        Debug.WriteLine(ex);
                    }
                }

            }

        }

        private void WriteThreadLog()
        {
            int minThreads = 0;
            int minCompletionThreads = 0;

            int availThreads = 0;
            int availCompletionThreads = 0;

            ThreadPool.SetMaxThreads(10, 10);
            ThreadPool.GetMinThreads(out minThreads, out minCompletionThreads);
            Debug.WriteLine($"min threads: {minThreads}; min completion threads: {minCompletionThreads}");


            ThreadPool.GetAvailableThreads(out availThreads, out availCompletionThreads);
            Debug.WriteLine($"available threads: {availThreads}; min completion threads: {availCompletionThreads}");
        }

    }
}
