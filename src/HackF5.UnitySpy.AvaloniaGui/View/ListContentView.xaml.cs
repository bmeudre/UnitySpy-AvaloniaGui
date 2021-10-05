namespace HackF5.UnitySpy.AvaloniaGui.View
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.AvaloniaGui.ViewModel;

    public class ListContentView : UserControl
    {
        private readonly ListBox itemsList;

        public ListContentView()
        {
            this.InitializeComponent();
            this.itemsList = this.FindControl<ListBox>("ItemsList");

            System.Console.WriteLine("Initialized List Content View");
        }

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 

        private void Control_OnMouseDoubleClick(object sender, object e)
        {
            if (!(this.itemsList.SelectedItem is ListItemViewModel item))
            {
                return;
            }

            if (!(this.DataContext is ListContentViewModel model))
            {
                return;
            }

            model.OnAppendToTrail(item.Index.ToString());
        }
    }
}
