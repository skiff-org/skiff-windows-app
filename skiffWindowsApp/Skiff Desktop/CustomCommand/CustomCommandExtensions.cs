using System;
using ToastNotifications;
using ToastNotifications.Core;

namespace CustomNotificationsExample.CustomCommand
{
    public static class CustomCommandExtensions
    {
        public static void ShowCustomCommand(this Notifier notifier, 
            string message, 
            Action<CustomCommandNotification> confirmAction, 
            Action<CustomCommandNotification> declineAction,
            MessageOptions messageOptions = null)
        {
            notifier.Notify(() => new CustomCommandNotification(message, confirmAction, declineAction, messageOptions));
        }
    }
}
