using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Windows;

namespace Skiff_Desktop
{
    public enum NotificationActionType
    {
        openThread,
        markAsRead,
        markAsSpam,
        sendToTrash,
    }

    internal class NotificationsController
    {
        private MainWindow _mainWindow;
        private TrayController _trayController;
        private MessageProcessor _messageProcessor;


        public NotificationsController(MainWindow mainWindow, TrayController trayController)
        {
            _mainWindow = mainWindow;
            _trayController = trayController;
        }

        public void SetMessageProcessor(MessageProcessor messageProcessor)
        { 
            _messageProcessor = messageProcessor;
        }

        internal void ShowToastNotification(string title, string message, string threadId)
        {
            // Microsoft.Toolkit.Uwp.Notifications requires net6.0-windows10.0.17763.0, so we are talking
            // about requiring Windows 10 version 1809 (also known as the October 2018 Update).
            // As fallback, we use NotifyIcon plain toast notification, which works fine, but is less feature rich.
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddArgument("action", NotificationActionType.openThread.ToString())
                    .AddArgument("threadId", threadId)
                    .AddButton(new ToastButton()
                        .SetContent("Mark as Read")
                        .AddArgument("action", NotificationActionType.markAsRead.ToString())
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("Spam")
                        .AddArgument("action", NotificationActionType.markAsSpam.ToString())
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("Send To Trash")
                        .AddArgument("action", NotificationActionType.sendToTrash.ToString())
                        .SetBackgroundActivation())
                    .Show();

                // Listen to notification activation
                ToastNotificationManagerCompat.OnActivated += OnToastNotificationActivated;
            }
            else
            {
                _trayController.ShowNotification(timeout: 2, title, message);
            }
        }

        private void OnToastNotificationActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

            // Need to dispatch to UI thread if performing UI operations
            Application.Current.Dispatcher.Invoke(delegate
            {
                string threadId = args["threadId"];
                string action = args["action"];
                NotificationActionType notificationAction = Enum.Parse<NotificationActionType>(action);

                switch (notificationAction)
                {
                    case NotificationActionType.openThread:
                        _messageProcessor.SendActionMessage(threadId, action);
                        _mainWindow.OpenWindow();
                        break;

                    case NotificationActionType.markAsRead:
                    case NotificationActionType.markAsSpam:
                    case NotificationActionType.sendToTrash:
                        _messageProcessor.SendActionMessage(threadId, action);
                        break;

                    default:
                        break;
                }
            });
        }
    }
}
