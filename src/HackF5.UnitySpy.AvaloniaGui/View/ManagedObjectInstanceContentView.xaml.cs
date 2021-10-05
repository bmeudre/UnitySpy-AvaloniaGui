namespace HackF5.UnitySpy.AvaloniaGui.View
{
    using Avalonia.Controls;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.AvaloniaGui.ViewModel;

    public class ManagedObjectInstanceContentView : UserControl
    {
        private readonly ListBox itemsList;

        public ManagedObjectInstanceContentView()
        {
            this.InitializeComponent();
            this.itemsList = this.FindControl<ListBox>("ItemsList");
        }

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 

        public void Control_OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!(this.itemsList.SelectedItem is InstanceFieldViewModel item))
            {
                return;
            }

            if (!(this.DataContext is ManagedObjectInstanceContentViewModel model))
            {
                return;
            }

            model.OnAppendToTrail(item.Name);
        }
    }
}