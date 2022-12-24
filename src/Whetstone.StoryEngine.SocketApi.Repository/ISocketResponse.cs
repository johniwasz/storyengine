using System.Net;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public interface ISocketResponse
    {
        /// <summary>
        /// Status code for the Response
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        ///  Body for the response
        /// </summary>
        string Body { get; set; }
    }
}
