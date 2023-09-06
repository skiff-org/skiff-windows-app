using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Security.Principal;

namespace Skiff_Desktop.Utilities
{
    public enum ThemeMode
    {
        Light = 0,
        Dark = 1,
    }

    public class ThemeWatcher
    {
        public ThemeMode Theme
        {
            get => _theme;
            private set {
                _theme = value;
                ThemeChanged?.Invoke(this, value);
            }
        }
        public event EventHandler<ThemeMode>? ThemeChanged;

        private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";
        private ManagementEventWatcher? watcher;
        private ThemeMode _theme;

        public void Start()
        {
            var currentUser = WindowsIdentity.GetCurrent();

            // Based on https://stackoverflow.com/a/69604613
            string query = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User?.Value,
                RegistryPath.Replace(@"\", @"\\"),
                RegistryValueName);

            try
            {
                Theme = GetCurrentTheme();

                // Listen for events
                watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, e) =>
                {
                    Theme = GetCurrentTheme();
                    Debug.WriteLine("System theme changed: " + Theme);
                };
                watcher.Start();
            }
            catch (Exception)
            {
                // This can fail on Windows 7
                Theme = ThemeMode.Light;
            }
        }

        public ThemeMode GetCurrentTheme()
        {
            ThemeMode theme = ThemeMode.Light;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser?.OpenSubKey(RegistryPath))
                {
                    object? value = key?.GetValue(RegistryValueName);
                    if (value == null)
                        return ThemeMode.Light;

                    theme = (int)value > 0 ? ThemeMode.Light : ThemeMode.Dark;
                }

                return theme;
            }
            catch (Exception)
            {
                return theme;
            }
        }
    }
}