namespace HackF5.UnitySpy.AvaloniaGui.View
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;

    public class TypeDefinitionView : UserControl
    {
        public TypeDefinitionView()
        {
            this.InitializeComponent();
            this.Initialized += (sender, args) => this.FindControl<TextBox>("PathTextBox").Focus();
        }

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 
    }
}