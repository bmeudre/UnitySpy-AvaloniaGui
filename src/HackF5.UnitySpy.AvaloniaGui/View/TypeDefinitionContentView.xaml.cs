namespace HackF5.UnitySpy.AvaloniaGui.View
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.AvaloniaGui.ViewModel;

    public class TypeDefinitionContentView : UserControl
    {
        private readonly ListBox itemsList;

        public TypeDefinitionContentView()
        {
            this.InitializeComponent();
            this.itemsList = this.FindControl<ListBox>("ItemsList");
        }

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 

        private void Control_OnMouseDoubleClick(object sender, object e)
        {
            if (!(this.itemsList.SelectedItem is StaticFieldViewModel item))
            {
                return;
            }

            if (!(this.DataContext is TypeDefinitionContentViewModel model))
            {
                return;
            }

            model.OnAppendToTrail(item.Name);
        }
    }
}