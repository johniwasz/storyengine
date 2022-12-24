namespace Whetstone.StoryEngine.Models.Integration
{

    /// <summary>
    /// Used by integration components that need to return a class along with a total count of items in a larger result set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CountResponse<T>
    {

        public CountResponse()
        {

        }


        public CountResponse(T value, int count)
        {
            Value = value;
            Count = count;
        }

        public T Value { get; set; }


        public int Count { get; set; }


    }
}
