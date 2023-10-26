using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Skiff_Desktop
{
    public enum MessageTypes
    {
        newMessageNotifications,
        unreadMailCount,
        notificationAction,
    }

    internal class MessageProcessor
    {
        private MainWindow _mainWindow;
        private NotificationsController _notificationsController;


        public MessageProcessor(MainWindow mainWindow, NotificationsController notificationsController)
        {
            _mainWindow = mainWindow;
            _notificationsController = notificationsController;
        }

        internal void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string rawMessage = e.TryGetWebMessageAsString();

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                    }
                };
                
                var receivedMessage = JsonSerializer.Deserialize<MessageWrapper>(rawMessage, options);

                switch (receivedMessage.MsgType)
                {
                    case MessageTypes.newMessageNotifications:
                        var notificationsPayload = JsonSerializer.Deserialize<NotificationDataWrapper>(receivedMessage.Data.ToString());
                        foreach (var notification in notificationsPayload.NotificationData)
                        {
                            Debug.WriteLine($"Displaying toast with title: {notification.Title} and body: {notification.Body}");
                            _notificationsController.ShowToastNotification(notification.Title, notification.Body, notification.ThreadId);
                        }
                        break;

                    case MessageTypes.unreadMailCount:
                        var counterPayload = JsonSerializer.Deserialize<UnreadCountDataWrapper>(receivedMessage.Data.ToString());
                        Debug.WriteLine($"Updating unread mail count: {counterPayload.UnreadCount}.");
                        _mainWindow.UpdateUnreadCount(counterPayload.UnreadCount);
                        break;

                    default:
                        Debug.WriteLine("Message type is not ‘newMessageNotifications’. Skipping.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to process message: {ex.Message}");
            }
        }

        internal void SendActionMessage(string threadId, string action)
        {
            var notificationData = new NotificationActionData()
            {
                Action = action,
                ThreadId = threadId,
            };

            var messageWrapper = new MessageWrapper()
            {
                MsgType = MessageTypes.notificationAction,
                Data = notificationData
            };

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            string stringData = JsonSerializer.Serialize(messageWrapper, options);

            _mainWindow.WebView2.CoreWebView2.PostWebMessageAsString(stringData);
        }


        #region Data Wrappers & Helpers

        [Serializable]
        public class MessageWrapper
        {
            [JsonPropertyName("type")]
            public MessageTypes MsgType { get; set; }
            [JsonPropertyName("data")]
            public Object Data { get; set; }
        }

        public class NotificationDataWrapper
        {
            [JsonPropertyName("notificationData")]
            public List<NotificationItem> NotificationData { get; set; }
        }

        public class NotificationItem
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }
            [JsonPropertyName("body")]
            public string Body { get; set; }
            [JsonPropertyName("threadID")]
            public string ThreadId { get; set; }
        }

        public class UnreadCountDataWrapper
        {
            [JsonPropertyName("numUnread")]
            public int UnreadCount { get; set; }
        }

        [Serializable]
        public class NotificationActionData
        {
            [JsonPropertyName("action")]
            public string Action { get; set; }
            [JsonPropertyName("threadId")]
            public string ThreadId { get; set; }
        }

        #endregion
    }
}
