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
    }

    internal class MessageProcessor
    {
        private MainWindow _mainWindow;


        public MessageProcessor(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
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
                
                var receivedMessage = JsonSerializer.Deserialize<ReceivedMessage>(rawMessage, options);

                switch (receivedMessage.MsgType)
                {
                    case MessageTypes.newMessageNotifications:
                        var notificationsPayload = JsonSerializer.Deserialize<NotificationDataWrapper>(receivedMessage.Data.ToString());
                        foreach (var notification in notificationsPayload.NotificationData)
                        {
                            Debug.WriteLine($"Displaying toast with title: {notification.Title} and body: {notification.Body}");
                            _mainWindow.ShowToastNotification(notification.Title, notification.Body);
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
                

        #region Data Wrappers & Helpers
                
        public class ReceivedMessage
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
        }

        public class UnreadCountDataWrapper
        {
            [JsonPropertyName("numUnread")]
            public int UnreadCount { get; set; }
        }

        #endregion
    }
}
