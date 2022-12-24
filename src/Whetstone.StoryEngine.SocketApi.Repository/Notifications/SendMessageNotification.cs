namespace Whetstone.StoryEngine.SocketApi.Repository.Notifications
{
    public class SendMessageNotification : BaseSocketNotification
    {
        public SendMessageNotification()
        {
            this.Message = "sendMessage";
        }
    }
}
