namespace Whetstone.StoryEngine.SocketApi.Repository.Responses
{
    public class SendMessageResponse : SocketResponse
    {
        public long ClientMsgId { get; set; }
        public string RequestId { get; set; }
    }
}
