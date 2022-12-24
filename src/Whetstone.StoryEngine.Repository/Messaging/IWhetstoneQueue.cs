using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public interface IWhetstoneQueue
    {
        Task AddMessageToQueueAsync<T>(string queueUrl, T message);

        Task<string> GetMessageFromQueueAsync(string queueUrl);
    }
}
