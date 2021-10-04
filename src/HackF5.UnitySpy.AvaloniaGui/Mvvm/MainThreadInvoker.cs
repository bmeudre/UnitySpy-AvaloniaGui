namespace HackF5.UnitySpy.AvaloniaGui.Mvvm
{
    using System;
    using System.Threading.Tasks;
    using Avalonia;
    using Avalonia.Threading;
    using JetBrains.Annotations;

    public class MainThreadInvoker : IMainThreadInvoker
    {
        public static IMainThreadInvoker Current { get; set; } = new MainThreadInvoker();

        public void InvokeOnMainThread([InstantHandle] [NotNull] Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Application.Current.CheckAccess())
            {
                action();
                return;
            }

            Dispatcher.UIThread.Post(action);
        }

        public async Task InvokeOnMainThreadAsync(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Application.Current.CheckAccess())
            {
                action();
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(action);
        }
    }
}