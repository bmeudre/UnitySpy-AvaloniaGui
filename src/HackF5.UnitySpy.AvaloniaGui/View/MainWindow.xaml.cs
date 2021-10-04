namespace HackF5.UnitySpy.AvaloniaGui.View
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;

    public class MainWindow : FluentWindow
    {
        public MainWindow()
        {   
            this.InitializeComponent();
        }       

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 
    }
}
