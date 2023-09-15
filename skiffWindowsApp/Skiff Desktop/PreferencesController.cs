using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace Skiff_Desktop
{
    internal class PreferencesController
    {
        public bool LaunchOnStartup { get; private set; }
        public bool StartMinimized { get; private set; }
        public bool MinimizeToTray { get; private set; }
        public bool CloseToTray { get; private set; }
        public WindowData WindowData { get; private set; }

        public Version Version { get; private set; }

        private MainWindow _mainWindow;

        private RegistryKey _startupKey;
        private RegistryKey _settingsPersistenceKey;
        private const string Minimize_To_Tray_Name = "MinimizeToTray";
        private const string Close_To_Tray_Name = "CloseToTray";
        private const string Start_Minimized_Name = "StartMinimized";
        private const string Window_Pos_And_State_Name = "WindowsPosAndState";


        public PreferencesController(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            _startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            LaunchOnStartup = _startupKey.GetValue(Application.ProductName) != null;

            _settingsPersistenceKey = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\{Application.CompanyName}");
            MinimizeToTray = bool.Parse(_settingsPersistenceKey.GetValue(Minimize_To_Tray_Name) as string ?? bool.FalseString);
            CloseToTray = bool.Parse(_settingsPersistenceKey.GetValue(Close_To_Tray_Name) as string ?? bool.FalseString);
            StartMinimized = bool.Parse(_settingsPersistenceKey.GetValue(Start_Minimized_Name) as string ?? bool.FalseString);
            WindowData = WindowData.Parse(_settingsPersistenceKey.GetValue(Window_Pos_And_State_Name) as string);

            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        public void SetLaunchOnStartup(bool enable)
        {
            if (enable)
                _startupKey.DeleteValue(Application.ProductName, false);
            else
                _startupKey.SetValue(Application.ProductName, Application.ExecutablePath);

            LaunchOnStartup = enable;
        }

        public void SetStartMinimized(bool enable)
        {
            _settingsPersistenceKey.SetValue(Start_Minimized_Name, enable);
            StartMinimized = enable;
        }

        public void SetMinimizeToTray(bool enable)
        {
            _settingsPersistenceKey.SetValue(Minimize_To_Tray_Name, enable);
            MinimizeToTray = enable;
        }

        public void SetCloseToTray(bool enable)
        {
            _settingsPersistenceKey.SetValue(Close_To_Tray_Name, enable);
            CloseToTray = enable;
        }

        public void SetWindowPosAndState(WindowData windowData)
        {
            _settingsPersistenceKey.SetValue(Window_Pos_And_State_Name, windowData.ToString());
        }
    }
}
