using StackExchange.Redis;
using System.Linq;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class RedisTest
    {



        //[Fact(DisplayName = "CacheTitle")]
        //public void CacheTitle()
        //{

        //    const string TestDataFile = @"localstore\stories\animalfarmtest\animalfarmtest.yaml";

        //    //const string TestDataFile = @"localstore\stories\test_adventure\test_adventure.yaml";

        //    string yamlRaw = File.ReadAllText(TestDataFile);

        //    var deser = YamlSerializationBuilder.GetYamlDeserializer();

        //    StoryTitle title = deser.Deserialize<StoryTitle>(yamlRaw);

        //    byte[] byteVal = null;
        //    using (MemoryStream memStream = new MemoryStream())
        //    {
        //        MessagePackSerializer.Serialize<StoryTitle>(memStream, title, MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);
        //        byteVal = memStream.ToArray();
        //    }

        //    StoryTitle titleReloaded = null;
        //    using (MemoryStream memStream = new MemoryStream(byteVal))
        //    {
        //        titleReloaded = MessagePack.MessagePackSerializer.Deserialize<StoryTitle>(memStream, MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);
        //        //retVal = ProtoBuf.Serializer.Deserialize<T>(memStream);
        //    }

        //    var ser = YamlSerializationBuilder.GetYamlSerializer();
        //    string yamlTitle = ser.Serialize(titleReloaded);

        //    File.WriteAllText("memtitle.yaml", yamlTitle);

        //}


        [Fact(DisplayName = "Clear Redis")]
        public void RedisRead()
        {

            string serverName = "dev-sbscache.prgrxr.0001.use1.cache.amazonaws.com";

            serverName = "dev-cache-sanjtest.prgrxr.ng.0001.use1.cache.amazonaws.com";

            var redisCon = ConnectionMultiplexer.Connect(string.Concat("allowAdmin=1, ", serverName));

            var redisServer = redisCon.GetServer(string.Concat(serverName, ":6379"));

            var redisDb = redisCon.GetDatabase();
            string instanceName = "sbs-engine";


            instanceName = string.Concat("dev", "-", instanceName);

            instanceName = "dev-sbs-engine";

            var keys = redisServer.Keys(pattern: instanceName + "*").ToArray();

            var sampleKeys = redisServer.Keys(pattern: "sample*").ToArray();

            sampleKeys = redisServer.Keys(pattern: "*").ToArray();

            redisDb.KeyDeleteAsync(sampleKeys);

            //redisDb.KeyDeleteAsync(keys);

            //  redisServer.FlushAllDatabases();

        }



    }
}
