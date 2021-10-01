namespace HackF5.UnitySpy.AvaloniaGui.Views
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;

    public class LinuxProcessSourceView : UserControl
    {
        public LinuxProcessSourceView()
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
