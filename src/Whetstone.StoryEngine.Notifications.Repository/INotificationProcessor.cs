using System.Threading.Tasks;

using Whetstone.StoryEngine.Models.Notifications;

namespace Whetstone.StoryEngine.Notifications.Repository
{
    public interface INotificationProcessor
    {
        Task ProcessNotification(NotificationRequest request);
    }
}
