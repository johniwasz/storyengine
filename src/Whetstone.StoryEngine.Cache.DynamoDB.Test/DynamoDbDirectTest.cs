using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Cache.DynamoDB.Test
{
    public class DynamoDbDirectTest
    {

        [Fact]
        public async Task GetCachedItemDynamoDb()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 5000;

            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;
            ThreadPool.SetMinThreads(10000, 10000);


            int timeout = 1500;
            var distCache = DynamoTestUtil.GetDynamoDbCache(3, timeout);


            List<Guid> userIds = new List<Guid>();
            for (int i = 0; i < 100; i++)
            {
                userIds.Add(Guid.NewGuid());

            }
          
            Stopwatch timer = new Stopwatch();

            var bag = new ConcurrentBag<TimingResponse>();
            var tasks = userIds.Select(async item =>
            {
               
                TimingResponse resp = new TimingResponse();
                resp.Exceptions = new List<Exception>();
                resp.IndividualTimings = new List<long>();

                //  Stopwatch timer = Stopwatch.StartNew();
                //var titleVersion = await distCache.GetAsync<TitleVersion>("title", "mapping:alexa-amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0");              

                long markZero = timer.ElapsedMilliseconds;

                var mapping = await distCache.GetAsync<TitleVersion>("title", "bogus");

                long firstMark = timer.ElapsedMilliseconds;
                resp.IndividualTimings.Add(firstMark - markZero);


             
                mapping = await distCache.GetAsync<TitleVersion>("title", "mapping:alexa-amzn1.ask.skill.f3a97670-6175-42a7-85b5-ca7f5a5be40d-live");

                long secondMark = timer.ElapsedMilliseconds;

                resp.IndividualTimings.Add(secondMark - firstMark);

                if (mapping == null)
                    resp.Exceptions.Add(new Exception("mapping:alexa-amzn1.ask.skill.f3a97670-6175-42a7-85b5-ca7f5a5be40d-live is null"));


                //  await Task.Delay(2000);

              
                var storyTitle = await distCache.GetAsync<StoryTitle>("title", "version:animalfarmpi-1.5");

                long thirdMark = timer.ElapsedMilliseconds;
                resp.IndividualTimings.Add(thirdMark - secondMark);


                //await Task.Delay(2000);
                if (storyTitle == null)
                    resp.Exceptions.Add(new Exception("version:animalfarmpi-1.5 is null"));


                //individualTimer.Restart();
                //var whetstoneTitle = await distCache.GetAsync<StoryTitle>("title", "version:whetstonetechnologies-0.3");
                //individualTimer.Stop();
                //resp.IndividualTimings.Add(individualTimer.ElapsedMilliseconds);
                //timer.Stop();


                //if (whetstoneTitle == null)
                //    resp.Exceptions.Add(new Exception("version:whetstonetechnologies-0.3 is null"));
                //else
                //    Debug.WriteLine("whetstone title is found");


               
                resp.Milliseconds = thirdMark - markZero;
                bag.Add(resp);


            });
            
            timer.Start();
            await Task.WhenAll(tasks);


            var count = bag.Count;

            var averageTime = bag.Select(x => x.IndividualTimings.Average()).Average();

            Console.WriteLine($"average time {averageTime}");

            var overTimes = bag.Where(x => x.Milliseconds > (timeout *4) || (x.Exceptions?.Any()).GetValueOrDefault(false));
            foreach (var overTime in overTimes)
            {
                Console.WriteLine(overTime.Milliseconds);
                foreach (Exception ex in overTime.Exceptions)
                {
                    Debug.WriteLine(ex);
                }
            }

        }


        [Fact]
        public async Task GetCachedItemDynamoDbSequential()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 5000;

            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;
            ThreadPool.SetMinThreads(10000, 10000);


            int timeout = 3000;
            var distCache = DynamoTestUtil.GetDynamoDbCache(2, timeout);

            var bag = new List<TimingResponse>();
            List<Guid> userIds = new List<Guid>();
            for (int i = 0; i < 100; i++)
            {
                userIds.Add(Guid.NewGuid());

                TimingResponse resp = new TimingResponse();
                resp.Exceptions = new List<Exception>();
                resp.IndividualTimings = new List<long>();

                Stopwatch timer = Stopwatch.StartNew();
                ////var titleVersion = await distCache.GetAsync<TitleVersion>("title", "mapping:alexa-amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0");
                Stopwatch individualTimer = new Stopwatch();
                //var mapping = await distCache.GetAsync<TitleVersion>("title", "bogus");
                //individualTimer.Stop();


                //resp.IndividualTimings.Add(individualTimer.ElapsedMilliseconds);


                individualTimer.Restart();
                var mapping = await distCache.GetAsync<TitleVersion>("title", "mapping:alexa-amzn1.ask.skill.f3a97670-6175-42a7-85b5-ca7f5a5be40d-live");
                individualTimer.Stop();
                resp.IndividualTimings.Add(individualTimer.ElapsedMilliseconds);

                if (mapping == null)
                    resp.Exceptions.Add(new Exception("mapping:alexa-amzn1.ask.skill.f3a97670-6175-42a7-85b5-ca7f5a5be40d-live is null"));


                //  await Task.Delay(2000);

                individualTimer.Restart();
                var storyTitle = await distCache.GetAsync<StoryTitle>("title", "version:animalfarmpi-1.5");
                individualTimer.Stop();
                resp.IndividualTimings.Add(individualTimer.ElapsedMilliseconds);


                //await Task.Delay(2000);
                if (storyTitle == null)
                    resp.Exceptions.Add(new Exception("version:animalfarmpi-1.5 is null"));


                //individualTimer.Restart();
                //var whetstoneTitle = await distCache.GetAsync<StoryTitle>("title", "version:whetstonetechnologies-0.3");
                //individualTimer.Stop();
                //resp.IndividualTimings.Add(individualTimer.ElapsedMilliseconds);
                //timer.Stop();


                //if (whetstoneTitle == null)
                //    resp.Exceptions.Add(new Exception("version:whetstonetechnologies-0.3 is null"));
                //else
                //    Debug.WriteLine("whetstone title is found");

                resp.Milliseconds = timer.ElapsedMilliseconds;
                bag.Add(resp);

            }

            
            var count = bag.Count;

            var averageTime = bag.Select(x => x.IndividualTimings.Average()).Average();

            Console.WriteLine($"average time {averageTime}");

            var overTimes = bag.Where(x => x.Milliseconds > (timeout * 4) || (x.Exceptions?.Any()).GetValueOrDefault(false));
            foreach (var overTime in overTimes)
            {
                Console.WriteLine(overTime.Milliseconds);
                foreach (Exception ex in overTime.Exceptions)
                {
                    Debug.WriteLine(ex);
                }
            }

        }


    }

[DebuggerDisplay("Millisecods: {Milliseconds}")]
    public class TimingResponse
    {
        public long Milliseconds { get; set; }

        public List<Exception> Exceptions { get; set; }

        public List<long> IndividualTimings { get; set; }

    }

}
