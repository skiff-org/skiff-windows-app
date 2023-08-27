using Microsoft.Web.WebView2.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using ToastNotifications.Messages.Core;
using ToastNotifications.Core;
using CustomNotificationsExample.CustomMessage;

namespace Skiff_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string baseURL = "https://app.skiff.com/";
        private Notifier _notifier;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBrowser();

            _notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: this,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(15),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));
                cfg.Dispatcher = System.Windows.Application.Current.Dispatcher;
                cfg.DisplayOptions.Width = 360;
                cfg.DisplayOptions.TopMost = true;
            });
        }

        private void ShowToastNotification(string title, string message)
        {
             var options = new MessageOptions { FreezeOnMouseEnter = true };
            _notifier.ShowCustomMessage(title, message, options);
        }

        protected override void OnClosed(EventArgs e)
        {
            _notifier.Dispose();
            base.OnClosed(e);
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string rawMessage = e.TryGetWebMessageAsString();
                // Deserialize the received message
                var receivedMessage = JsonSerializer.Deserialize<ReceivedMessage>(rawMessage);
                // Check if the type is "newMessageNotifications"
                if (receivedMessage.Type == "newMessageNotifications")
                {
                    foreach (var notification in receivedMessage.Data.NotificationData)
                    {
                        Debug.WriteLine($"Displaying toast with title: { notification.Title} and body: { notification.Body}");

                        // show toast
                        // ToastNotification toast = new ToastNotification()
                        ShowToastNotification(notification.Title, notification.Body);
                    }
                }
                else
                {
                    Debug.WriteLine("Message type is not ‘newMessageNotifications’. Skipping.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to process message: { ex.Message}");
            }
        }

        public class ReceivedMessage
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("data")]
            public NotificationDataWrapper Data { get; set; }
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

        private async Task InitializeBrowser()
        {
            // When installing the app webview attempts to create a folder with cache folders in root app directory
            // which crashes the app, to fix this we must create the webview with the user data path set to temp where we
            // have permissions
            var userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\skiff";
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await WebView2.EnsureCoreWebView2Async(env);
            WebView2.Source = new Uri(baseURL);

            WebView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            WebView2.CoreWebView2.Settings.IsScriptEnabled = true;
            WebView2.CoreWebView2.Settings.AreDevToolsEnabled = false;
            WebView2.CoreWebView2.Settings.AreHostObjectsAllowed = false;
            WebView2.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            WebView2.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
            WebView2.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
            WebView2.CoreWebView2.Settings.IsStatusBarEnabled = false;

            // this is needed to allow the webview to communicate with the app
            // right now, only for sending notifications
            WebView2.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            WebView2.CoreWebView2.Settings.IsWebMessageEnabled = true; // Make sure this is set to true

            await WebView2.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.IsSkiffWindowsDesktop = true;");
        }

        private async void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            WebView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            // In the link is a skiff link open in current window
            if (e.Uri.StartsWith(baseURL))
            {
                // If the link is a skiff link, open in the current window.
                e.Handled = true;
                WebView2.CoreWebView2.Navigate(e.Uri);
            }
            else if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    OpenInDefaultBrowser(e.Uri);
                }
            }
        }
        private void OpenInDefaultBrowser(string uri)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }



        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (!e.Uri.StartsWith(baseURL))
            {
                // Do not allow navigation to url's that are not skiff
                e.Cancel = true;

                OpenInDefaultBrowser(e.Uri);
            }
        }
    }
}
