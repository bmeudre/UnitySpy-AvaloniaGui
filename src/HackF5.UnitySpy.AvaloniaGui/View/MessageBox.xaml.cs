namespace HackF5.UnitySpy.AvaloniaGui
{
    using System;
    using System.Threading.Tasks;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Platform;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Avalonia.Media.Imaging;

    public enum MessageBoxButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No
    }

    public enum MessageBoxImage
    {   
        Asterisk, 
        Error,
        Exclamation,
        Hand,
        Information,
        None,
        Question,
        Stop,
        Warning
    }

    public class MessageBox : Avalonia.Controls.Window
    {        
        
        protected Image _Image;

        public MessageBoxImage Image 
        {
            set 
            {
                if(value == MessageBoxImage.None)
                {
                    _Image.IsVisible = false;
                }
                else
                {
                    _Image.IsVisible = true;
                    string uri;
                    switch(value) 
                    {
                        case MessageBoxImage.Asterisk: uri = "/Assets/info.png"; break;
                        case MessageBoxImage.Error: uri = "/Assets/error.png"; break;
                        case MessageBoxImage.Exclamation: uri = "/Assets/warning.png"; break;
                        case MessageBoxImage.Hand: uri = "/Assets/error.png"; break;
                        case MessageBoxImage.Information: uri = "/Assets/info.png"; break;
                        case MessageBoxImage.Question: uri = "/Assets/question.png"; break;
                        case MessageBoxImage.Stop: uri = "/Assets/error.png"; break;
                        case MessageBoxImage.Warning: uri = "/Assets/warning.png"; break;
                        default: throw new NotSupportedException("Message Box Image type not supported");
                    }
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    _Image.Source = new Bitmap(assets.Open(new Uri(uri)));
                }                
            }
        }

        protected TextBlock _MessageTxtBlck;
 
        protected Button _OkBtn;

        protected Button _CancelBtn;

        protected Button _YesBtn;

        protected Button _NoBtn;

        protected Action<MessageBoxResult> _OnClose;

        protected Window _Parent;

        public MessageBoxButton Buttons 
        {
            set 
            {
                _OkBtn.IsVisible =     value == MessageBoxButton.OK       || value == MessageBoxButton.OKCancel;
                _CancelBtn.IsVisible = value == MessageBoxButton.OKCancel || value == MessageBoxButton.YesNoCancel;
                _YesBtn.IsVisible =    value == MessageBoxButton.YesNo    || value == MessageBoxButton.YesNoCancel;
                _NoBtn.IsVisible =     value == MessageBoxButton.YesNo    || value == MessageBoxButton.YesNoCancel;
            }
        }

        public MessageBox() 
        {
            AvaloniaXamlLoader.Load(this);
            _Image = this.FindControl<Image>("Image");
            _MessageTxtBlck = this.FindControl<TextBlock>("Message");
            _OkBtn = this.FindControl<Button>("Ok");
            _CancelBtn = this.FindControl<Button>("Cancel");
            _YesBtn = this.FindControl<Button>("Yes");
            _NoBtn = this.FindControl<Button>("No");
        }

        protected void ShowDialog(Window parent) {
            if (parent != null) 
            {
                base.ShowDialog(parent);
            }
            else 
            {
                Show();
            } 
        }

        public void Ok_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            Close(MessageBoxResult.Ok);
        }

        public void Cancel_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            Close(MessageBoxResult.Cancel);
        }

        public void Yes_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            Close(MessageBoxResult.Yes);
        }

        public void No_Click(object sender, RoutedEventArgs routedEventArgs) 
        {
            Close(MessageBoxResult.No);
        }

        protected void Close(MessageBoxResult result) 
        {
            base.Close(result);
            _OnClose?.Invoke(result);
        }

        public static void Show(string message, string title, Action<MessageBoxResult> onClose)
        {
            Show(null, message, title, MessageBoxButton.OK, MessageBoxImage.None, onClose);
        }

        public static void Show(string message, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            Show(null, message, title, buttons, image, null);
        }

        public static void Show(Window parent, string message, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, Action<MessageBoxResult> onClose = null) 
        {
            GetInstance(message, title, buttons, image, onClose).ShowDialog(parent);            
        }

        public static Task<MessageBoxResult> ShowAsync(string text, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None) 
        {
            return ShowAsync(null, text, title, buttons, image);
        }

        public static Task<MessageBoxResult> ShowAsync(Window parent, string text, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            if(parent == null)
            {
                var taskCompletionSource = new TaskCompletionSource<MessageBoxResult>();            
                MessageBox messageBox = GetInstance(text, title, buttons, image, result => taskCompletionSource.TrySetResult(result));
                messageBox.ShowDialog(parent);
                return taskCompletionSource.Task;
            }
            else 
            {
                MessageBox messageBox = GetInstance(text, title, buttons, image);
                return messageBox.ShowDialog<MessageBoxResult>(parent);
            }
        }

        private static MessageBox GetInstance(string message, string title = "Title", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, Action<MessageBoxResult> onClose = null) 
        {       
            MessageBox messageBox = new MessageBox();

            messageBox.Image = image;
            messageBox.Title = title;
            messageBox._MessageTxtBlck.Text = message;
            messageBox.Buttons = buttons;
            messageBox._OnClose = onClose;

            return messageBox;
        }

    }

}