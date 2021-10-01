﻿namespace HackF5.UnitySpy.AvaloniaGui.Views
{
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.AvaloniaGui.ViewModels;

    public class MainWindow : FluentWindow
    {
        public MainWindow()
        {   
            this.InitializeComponent();
            LinuxProcessSourceView  linuxProcessSourceView = this.Find<LinuxProcessSourceView>("LinuxProcessSourceView");
            linuxProcessSourceView.DataContext = new LinuxProcessSourceViewModel(this);
        }       

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 
    }
}
