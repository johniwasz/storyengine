using System;
using System.Net;
namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public class SocketServiceException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public SocketServiceException()
        {
        }

        public SocketServiceException( string message ) :
            base(message)
        {
        }

        public SocketServiceException(HttpStatusCode statusCode) :
            base()
        {
            StatusCode = statusCode;
        }

        public SocketServiceException(string message, HttpStatusCode statusCode) :
            base(message)
        {
            StatusCode = statusCode;
        }

        public SocketServiceException(string message, HttpStatusCode statusCode, Exception innerException) :
            base(message, innerException)
        {
            StatusCode = statusCode;
        }

    }
}
