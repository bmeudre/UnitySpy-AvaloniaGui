namespace HackF5.UnitySpy.AvaloniaGui.Mvvm
{
    using System;
    using System.Threading.Tasks;

    public interface IMainThreadInvoker
    {
        void InvokeOnMainThread(Action action);

        Task InvokeOnMainThreadAsync(Action action);
    }
}