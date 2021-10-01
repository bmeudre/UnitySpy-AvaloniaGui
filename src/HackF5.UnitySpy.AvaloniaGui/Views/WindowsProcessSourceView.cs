namespace HackF5.UnitySpy.AvaloniaGui.Views
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;

    public class WindowsProcessSourceView : UserControl
    {
        private readonly ComboBox processesComboBox;

        public WindowsProcessSourceView()
        {   
            this.InitializeComponent();
            processesComboBox = this.Find<ComboBox>("processesComboBox");
        }       

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 
    }
}
