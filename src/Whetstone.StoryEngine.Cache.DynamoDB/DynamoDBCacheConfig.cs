namespace Whetstone.StoryEngine.Cache.DynamoDB
{
    public class DynamoDBCacheConfig
    {
        public string TableName { get; set; }

        public int MaxRetries { get; set; }

        public int Timeout { get; set; }

    }
}
