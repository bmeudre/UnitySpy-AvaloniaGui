namespace HackF5.UnitySpy.AvaloniaGui.ViewModels
{
    using System;
    using HackF5.UnitySpy.AvaloniaGui.Views;
    using ReactiveUI;

    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel()
        {
        }       

        public bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT; 

        public bool IsLinux => Environment.OSVersion.Platform == PlatformID.Unix;            
    }
}
