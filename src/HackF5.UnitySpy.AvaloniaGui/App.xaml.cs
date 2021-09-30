namespace HackF5.UnitySpy.AvaloniaGui
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.AvaloniaGui.ViewModels;
    using HackF5.UnitySpy.AvaloniaGui.Views;

    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.DataContext = new MainWindowViewModel(mainWindow);
                desktop.MainWindow = mainWindow;
            }
            
            var theme = new Avalonia.Themes.Default.DefaultTheme();
            theme.TryGetResource("Button", out _);

            base.OnFrameworkInitializationCompleted();
        }
    }
}
