using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Skiff_Desktop
{
    internal class TrayController
    {
        private NotifyIcon _trayIcon;
        private ToolStripMenuItem _traySettingMenuItem;
        private Icon _iconNormal;
        private Icon _iconUnreadDot;
        private MainWindow _mainWindow;


        public TrayController(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _mainWindow.StateChanged += OnWindowStateChanged;
            _mainWindow.UnreadCounterChanged += UpdateTrayInfo;

            InitIconsBitmaps();
            InitTrayIcon();
            UpdateTrayInfo();
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
            _traySettingMenuItem = new ToolStripMenuItem("Minimize To Tray", null, OnMinimizeSettingChange);
            _traySettingMenuItem.Checked = true;

            _trayIcon = new NotifyIcon()
            {
                Icon = _iconNormal,
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items =
                    {
                        new ToolStripMenuItem("Show", null, OnOpenFromTray),
                        _traySettingMenuItem,
                        new ToolStripSeparator(),                           // Remove for release.
                        new ToolStripMenuItem("Test Dot", null, TestDot),   // Remove for release.
                        new ToolStripMenuItem("Clear Dot", null, ClearDot),     // Remove for release.
                        new ToolStripSeparator(),
                        new ToolStripMenuItem("Quit", null, OnCloseFromTray)
                    }
                },
                Visible = true,
            };
            _trayIcon.DoubleClick += OnOpenFromTray;
        }

        private void UpdateTrayInfo()
        {
            if (_mainWindow.UnreadCount > 0)
            {
                _trayIcon.Icon = _iconUnreadDot;
                _trayIcon.Text = $"Skiff Desktop ({_mainWindow.UnreadCount} Unread Emails)";
            }
            else
            {
                _trayIcon.Icon = _iconNormal;
                _trayIcon.Text = "Skiff Desktop";
            }
        }

        private void OnMinimizeSettingChange(object? sender, EventArgs e)
        {
            _traySettingMenuItem.Checked = !_traySettingMenuItem.Checked;
        }

        private void OnOpenFromTray(object? sender, EventArgs e)
        {
            if (_mainWindow.WindowState == WindowState.Minimized)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            if (_mainWindow.WindowState == WindowState.Minimized && _traySettingMenuItem.Checked)
            {
                _mainWindow.Hide();
            }
        }

        private void OnCloseFromTray(object? sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            _mainWindow.Close();
        }

        // Remove for release.
        private void TestDot(object? sender, EventArgs e)
        {
            _mainWindow.UpdateUnreadCount(_mainWindow.UnreadCount + 1);
            UpdateTrayInfo();
        }

        // Remove for release.
        private void ClearDot(object? sender, EventArgs e)
        {
            _mainWindow.UpdateUnreadCount(0);
            UpdateTrayInfo();
        }
    }
}
