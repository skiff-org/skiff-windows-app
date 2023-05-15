# Skiff Windows App
Native Windows WPF Application

# Installation
1. Install Visual Studio 2022
2. When installing Visual Studio 2022 make sure to include
    1. ASP.NET and web development is Web & Cloud
    ![install1](../apps/skiff-wiki/images/window_vs_1.PNG)
    2. Desktop development with C++, .Net desktop development and Universal Windows Platform Development (UWP might not be needed but doesnt hurt to have)
    ![install2](../apps/skiff-wiki/images/windows_vs_2.PNG)
    3. Make sure in installation details to include C++ (v143) and Windows SDK's under UWP development as-well as Windows App SDK C# Templates under .NET Desktop development
    ![install3](../apps/skiff-wiki/images/windows_vs_3.PNG)
3. Open the solution `Skiff Desktop.sln` with Visual Studio 2022, in the top toolbar select `Extentions` -> `Manage extentions` and install `Microsoft Visual Studio Installer Projects 2022`
![install4](../apps/skiff-wiki/images/windows_vs_4.PNG)
4. Restart Visual Studio 2022 (Might to reload projects)
5. Run the app with the play botton at top center of the screen

# Webview code
The webview code is located in `MainWindow.xalm.cs`
