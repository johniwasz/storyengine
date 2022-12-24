using System.Net;

namespace Whetstone.StoryEngine.SocketApi.Repository.Responses
{
    public class SocketResponse : ISocketResponse
    {
        public SocketResponse()
        {
        }

        private HttpStatusCode _statusCode;
        private string _body;

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }

        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }
    }
}
