namespace HackF5.UnitySpy.AvaloniaGui.View
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;

    public class AssemblyImageView : UserControl
    {
        private readonly ListBox definitionsList;

        public AssemblyImageView()
        {
            this.InitializeComponent();
            this.definitionsList = this.FindControl<ListBox>("DefinitionsList");
        } 

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 

        private void DefinitionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.definitionsList.ScrollIntoView(this.definitionsList.SelectedItem);
        }
    }
}