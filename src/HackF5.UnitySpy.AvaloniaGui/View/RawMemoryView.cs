namespace HackF5.UnitySpy.AvaloniaGui
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;

    public class RawMemoryView : FluentWindow
    {                
        public RawMemoryView() 
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