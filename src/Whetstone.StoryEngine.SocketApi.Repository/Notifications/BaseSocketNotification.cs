using System.Net;

namespace Whetstone.StoryEngine.SocketApi.Repository.Notifications
{
    public class BaseSocketNotification
    {
        public BaseSocketNotification()
        {
        }

        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string RequestId { get; set; }
        public string UserId { get; set; }
        public string Body { get; set; }

    }
}
