namespace Whetstone.StoryEngine.Cache
{
    public class CacheTableAttributes
    {
        /// <summary>
        /// Parallel read capacity
        /// </summary>
        public const int ReadCapacityUnits = 10;

        /// <summary>
        /// Parallel write capacity 
        /// </summary>
        public const int WriteCapacityUnits = 5;

        /// <summary>
        /// Creates a dynamo db on startup
        /// </summary>
        public const bool CreateTableOnStartUp = true;

        /// <summary>
        /// Default time to live (TTL) in minutes for 1 month
        /// </summary>
        public const long Ttl = 43800; 
    }
}