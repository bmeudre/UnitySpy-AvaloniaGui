namespace HackF5.UnitySpy.AvaloniaGui.Mvvm
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Avalonia.Metadata;

    public class ExtendedContentControl : ContentControl
    {
        public static readonly DirectProperty<ExtendedContentControl, object> ExtendedContentProperty = 
            AvaloniaProperty.RegisterDirect<ExtendedContentControl, object>(
                nameof(ExtendedContent),
                o => o.ExtendedContent,
                OnExtendedContentChanged);

        public object ExtendedContent
        {
            get => (object)this.GetValue(ExtendedContentControl.ExtendedContentProperty);
            set => this.SetValue(ExtendedContentControl.ExtendedContentProperty, value);
        }

        private static void OnExtendedContentChanged(ExtendedContentControl control, object value)
        {
            control.Content = ViewLocator.GetViewFor(value);
        }
    }
}