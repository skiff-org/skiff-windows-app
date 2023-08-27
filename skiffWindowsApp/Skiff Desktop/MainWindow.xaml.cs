using Microsoft.Web.WebView2.Core;
using System;
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

namespace Skiff_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string baseURL = "https://app.skiff.com/";

        public MainWindow()
        {
            InitializeComponent();
            InitializeBrowser();
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
            WebView2.CoreWebView2.Settings.IsWebMessageEnabled = false;
            WebView2.CoreWebView2.Settings.AreHostObjectsAllowed = false;
            WebView2.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            WebView2.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
            WebView2.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
            WebView2.CoreWebView2.Settings.IsStatusBarEnabled = false;

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
                WebView2.CoreWebView2.Navigate(e.Uri);
            }
            else if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    using var _ = Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = e.Uri,
                    });
                }
            }
        }

        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (!e.Uri.StartsWith(baseURL))
            {
                // Do not allow navigation to url's that are not skiff
                e.Cancel = true;
            }
        }
    }
}
