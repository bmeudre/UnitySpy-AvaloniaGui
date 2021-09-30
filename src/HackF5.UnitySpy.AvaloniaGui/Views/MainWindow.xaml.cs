namespace HackF5.UnitySpy.AvaloniaGui.Views
{
    using System;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Avalonia.Diagnostics;

    public class MainWindow : FluentWindow
    {
        private readonly ComboBox processesComboBox;

        public MainWindow()
        {   
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
            processesComboBox = this.Find<ComboBox>("processesComboBox");
        }        
    }
}
