namespace HackF5.UnitySpy.AvaloniaGui
{
    using Autofac;
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.AvaloniaGui.ViewModel;
    using HackF5.UnitySpy.AvaloniaGui.View;

    public class App : Application
    {
        private IContainer container;
        
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

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(this.GetType().Assembly);
            this.container = builder.Build();
            
            var theme = new Avalonia.Themes.Default.DefaultTheme();
            theme.TryGetResource("Button", out _);

            base.OnFrameworkInitializationCompleted();
        }
    }
}
