using System;
namespace Whetstone.StoryEngine.Models.Notifications

{
    public enum NotificationRequestType
    {
        SendNotificationToClient,
        SendNotificationsForClient
    }

    public enum NotificationDataType
    {
        None,
        AddAudioFileToProject
    }
}
