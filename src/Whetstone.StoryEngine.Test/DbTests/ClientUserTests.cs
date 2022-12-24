using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;


namespace Whetstone.StoryEngine.Test.DbTests
{
    public class ClientUserTests
    {

        [Fact]
        public async Task LoadDynamoDbStreamTest()
        {


            RegionEndpoint endpoint = RegionEndpoint.USEast1;
            string streamArn = "arn:aws:dynamodb:us-east-1:940085449815:table/Whetstone-DynamoDbStore-Dev-UserTable-1VS3IYWFNK5J7/stream/2019-07-16T12:49:05.648";

            using (var streamClient = new AmazonDynamoDBStreamsClient(endpoint))
            {

                DescribeStreamRequest descStreamReq = new DescribeStreamRequest
                {
                    StreamArn = streamArn,
                    Limit = 100
                };


                var descResp = await streamClient.DescribeStreamAsync(descStreamReq);


                GetShardIteratorRequest shardRequest = new GetShardIteratorRequest();



                foreach (var shard in descResp.StreamDescription.Shards)
                {

                    Debug.WriteLine(shard.ShardId);

                    shardRequest = new GetShardIteratorRequest
                    {
                        StreamArn = streamArn,
                        ShardIteratorType = ShardIteratorType.LATEST,
                        ShardId = shard.ShardId
                    };

                    var shardResponse = await streamClient.GetShardIteratorAsync(shardRequest);

                    await ProcessShardIterator(shardResponse.ShardIterator);

                }





            }

        }

        private async Task ProcessShardIterator(string shardIterator)
        {

            GetRecordsRequest recordRequest = new GetRecordsRequest
            {
                ShardIterator = shardIterator,

                Limit = 100
            };
            RegionEndpoint endpoint = RegionEndpoint.USEast1;
            using (var streamClient = new AmazonDynamoDBStreamsClient(endpoint))
            {
                var recResponse = await streamClient.GetRecordsAsync(recordRequest);




                foreach (var record in recResponse.Records)
                {

                    Debug.WriteLine(record.EventSource);

                }

                while (!string.IsNullOrWhiteSpace(recResponse.NextShardIterator))
                {
                    recordRequest.ShardIterator = recResponse.NextShardIterator;
                    Debug.WriteLine($"Next iterator: {recordRequest.ShardIterator}");

                    recResponse = await streamClient.GetRecordsAsync(recordRequest);

                    foreach (var record in recResponse.Records)
                    {

                        Debug.WriteLine(record.EventSource);

                    }


                }
            }

        }

    }
}
