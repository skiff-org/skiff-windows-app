using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace Skiff_Desktop
{
    internal class TrayController
    {
        private NotifyIcon _trayIcon;
        private Icon _iconNormal;
        private Icon _iconUnreadDot;
        private ToolStripMenuItem _minimizeToTrayPreferenceMenuItem;
        private ToolStripMenuItem _closeToTrayPreferenceMenuItem;
        private ToolStripMenuItem _startupPreferenceMenuItem;
        private ToolStripMenuItem _startMinimizedPreferenceMenuItem;
        private ToolStripMenuItem _preferencesMenuItem;
        private bool _closeFromTrayMenu;
        private MainWindow _mainWindow;
        private PreferencesController _preferencesController;


        public TrayController(MainWindow mainWindow, PreferencesController preferencesController)
        {
            _mainWindow = mainWindow;
            _mainWindow.StateChanged += OnWindowStateChanged;
            _mainWindow.UnreadCounterChanged += UpdateTrayInfo;
            _mainWindow.Closing += OnWindowClosing;

            _preferencesController = preferencesController;

            InitIconsBitmaps();
            InitTrayIcon();
            UpdateTrayInfo();
        }

        internal void ShowNotification(int timeout, string title, string message)
        {
            _trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Info);
        }

        private void InitIconsBitmaps()
        {
            IntPtr iconHandle = Properties.Resources.logo.GetHicon();
            _iconNormal = Icon.FromHandle(iconHandle);

            Color niceRed = Color.FromArgb(255, 240, 71, 71);
            Brush bgColor = new SolidBrush(niceRed);
            Bitmap bitmap = Properties.Resources.logo; // bitmap.Width: 32
            Graphics graphics = Graphics.FromImage(bitmap);
            int diameter = 14;
            int coord = 18;
            Rectangle dotPosAndSize = new Rectangle(coord, coord, diameter, diameter);
            graphics.FillEllipse(bgColor, dotPosAndSize);
            // If at some point we want to render the counter, we will need this, but will be redraw each time, so wont go here.
            //graphics.DrawString(_count.ToString(), new Font("Arial Unicode MS", 11), Brushes.White, new PointF(coord, coord));
            _iconUnreadDot = Icon.FromHandle(bitmap.GetHicon());
        }

        private void InitTrayIcon()
        {
            _minimizeToTrayPreferenceMenuItem = new ToolStripMenuItem("Minimize to tray", null, OnMinimizePreferenceChange);
            _minimizeToTrayPreferenceMenuItem.Checked = _preferencesController.MinimizeToTray;

            _closeToTrayPreferenceMenuItem = new ToolStripMenuItem("Close to tray", null, OnClosePreferenceChange);
            _closeToTrayPreferenceMenuItem.Checked = _preferencesController.CloseToTray;

            _startupPreferenceMenuItem = new ToolStripMenuItem("Launch on startup", null, OnLaunchOnStartupPreferenceChange);
            _startupPreferenceMenuItem.Checked = _preferencesController.LaunchOnStartup;

            _startMinimizedPreferenceMenuItem = new ToolStripMenuItem("Start minimized", null, OnStartMinimizedPreferenceChange);
            _startMinimizedPreferenceMenuItem.Checked = _preferencesController.StartMinimized;

            _preferencesMenuItem = new ToolStripMenuItem("Preferences");
            _preferencesMenuItem.DropDownItems.Add(_minimizeToTrayPreferenceMenuItem);
            _preferencesMenuItem.DropDownItems.Add(_closeToTrayPreferenceMenuItem);
            _preferencesMenuItem.DropDownItems.Add(_startupPreferenceMenuItem);
            _preferencesMenuItem.DropDownItems.Add(_startMinimizedPreferenceMenuItem);

            _trayIcon = new NotifyIcon()
            {
                Icon = _iconNormal,
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items =
                    {
                        new ToolStripMenuItem("Show", null, OpenFromTray),
                        _preferencesMenuItem,
                        new ToolStripMenuItem("Check for updates", null, CheckForUpdates),
                        new ToolStripMenuItem("About", null, About),
                        new ToolStripSeparator(),
                        new ToolStripMenuItem("Quit", null, CloseFromTray)
                    }
                },
                Visible = true,
            };
            _trayIcon.DoubleClick += OpenFromTray;
        }

        private void OnLaunchOnStartupPreferenceChange(object? sender, EventArgs e)
        {
            _preferencesController.SetLaunchOnStartup(_startupPreferenceMenuItem.Checked);
            _startupPreferenceMenuItem.Checked = !_startupPreferenceMenuItem.Checked;
        }

        private void OnStartMinimizedPreferenceChange(object? sender, EventArgs e)
        {
            _startMinimizedPreferenceMenuItem.Checked = !_startMinimizedPreferenceMenuItem.Checked;
            _preferencesController.SetStartMinimized(_startMinimizedPreferenceMenuItem.Checked);
        }

        [Serializable]
        public class UpdateData
        {
            [JsonPropertyName("tag_name")]
            public string Tag { get; set; }
            [JsonPropertyName("html_url")]
            public string Url { get; set; }
            [JsonPropertyName("published_at")]
            public DateTime ReleaseDate { get; set; }

            public Version Version { get { return Version.Parse(Tag); } }
        }

        private async void CheckForUpdates(object? sender, EventArgs e)
        {
            var timeoutSource = new CancellationTokenSource(5000);
            var url = "https://api.github.com/repos/skiff-org/skiff-windows-app/releases";
            var response = await _mainWindow.HttpClient.GetAsync(url, timeoutSource.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                var updateData = JsonSerializer.Deserialize<List<UpdateData>>(content);

                // Sort releases by date and retrieve latest.
                updateData.Sort((a, b) => b.ReleaseDate.CompareTo(a.ReleaseDate));
                var latestRelease = updateData.FirstOrDefault();

                bool updateAvailable = latestRelease.Version > _preferencesController.Version;
                string msgBoxContent = $"Current version {_preferencesController.Version} is up to date.";
                MessageBoxButton msgBoxButtons = MessageBoxButton.OK;

                if (updateAvailable)
                {
                    msgBoxContent =
                    $"Current version {_preferencesController.Version} is outdated. \n" +
                    $"\n" +
                    $"Version {latestRelease.Version} is available. \n" +
                    $"Do you want to open the download page?";

                    msgBoxButtons = MessageBoxButton.YesNo;
                }

                var result = System.Windows.MessageBox.Show(
                    msgBoxContent,
                    "About Skiff Desktop",
                    msgBoxButtons,
                    MessageBoxImage.None);

                if (result == MessageBoxResult.Yes)
                {
                    _mainWindow.OpenInDefaultBrowser(latestRelease.Url);
                }
            }
        }

        private void About(object? sender, EventArgs e)
        {
            string content =
                $"Skiff Desktop\n" +
                $"Version {_preferencesController.Version}\n" +
                $"\n" +
                $"Contact Skiff team:\n" +
                $"https://skiff.com\n" +
                $"support@skiff.org";

            System.Windows.MessageBox.Show(
                content,
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.None);
        }

        private void UpdateTrayInfo()
        {
            if (_mainWindow.UnreadCount > 0)
            {
                _trayIcon.Icon = _iconUnreadDot;
                _trayIcon.Text = $"Skiff Desktop ({_mainWindow.UnreadCount} Unread)";
            }
            else
            {
                _trayIcon.Icon = _iconNormal;
                _trayIcon.Text = "Skiff Desktop";
            }
        }

        private void OnMinimizePreferenceChange(object? sender, EventArgs e)
        {
            _minimizeToTrayPreferenceMenuItem.Checked = !_minimizeToTrayPreferenceMenuItem.Checked;
            _preferencesController.SetMinimizeToTray(_minimizeToTrayPreferenceMenuItem.Checked);
        }

        private void OnClosePreferenceChange(object? sender, EventArgs e)
        {
            _closeToTrayPreferenceMenuItem.Checked = !_closeToTrayPreferenceMenuItem.Checked;
            _preferencesController.SetCloseToTray(_closeToTrayPreferenceMenuItem.Checked);
        }

        private void OpenFromTray(object? sender, EventArgs e)
        {
            _mainWindow.OpenWindow();
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            if (_mainWindow.WindowState == WindowState.Minimized && _minimizeToTrayPreferenceMenuItem.Checked)
            {
                _mainWindow.Hide();
            }
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (_closeToTrayPreferenceMenuItem.Checked && !_closeFromTrayMenu)
            {
                e.Cancel = true;
                _mainWindow.WindowState = WindowState.Minimized;
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.StateChanged -= OnWindowStateChanged;
                _mainWindow.UnreadCounterChanged -= UpdateTrayInfo;
                _mainWindow.Closing -= OnWindowClosing;

                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
        }

        private void CloseFromTray(object? sender, EventArgs e)
        {
            _closeFromTrayMenu = true;
            _mainWindow.Close();
        }
    }
}
