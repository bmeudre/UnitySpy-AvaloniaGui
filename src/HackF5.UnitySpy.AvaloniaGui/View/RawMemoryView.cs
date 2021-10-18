namespace HackF5.UnitySpy.AvaloniaGui
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.AvaloniaGui.ViewModel;

    public class RawMemoryView : Window
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