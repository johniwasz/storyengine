using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public enum NotificationsDispatchTypeEnum
    {
        Direct =1,
        StepFunction =2
    }


    public interface INotificationDispatcher
    {

        Task DispatchNotificationAsync(INotificationRequest notificationRequest);

    }
}
