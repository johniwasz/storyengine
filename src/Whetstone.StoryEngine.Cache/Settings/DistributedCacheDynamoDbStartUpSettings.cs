namespace Whetstone.StoryEngine.Cache.Settings
{
    public class DistributedCacheDynamoDbStartUpSettings
    {
        /// <summary>
        /// Create dynamodb on startup
        /// </summary>
        public bool CreateDbOnStartUp { get; set; }

        /// <summary>
        /// Read capcity limit (Parallel)
        /// </summary>
        public int ReadCapacityUnits { get; set; }

        /// <summary>
        /// Wrtie capcity limit (Parallel)
        /// </summary>
        public int WriteCapacityUnits { get; set; }
    }
}