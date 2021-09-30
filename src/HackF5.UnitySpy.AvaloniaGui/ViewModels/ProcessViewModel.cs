namespace HackF5.UnitySpy.AvaloniaGui.ViewModels
{
    using System.Diagnostics;
    using ReactiveUI;

    public class ProcessViewModel : ReactiveObject
    {
        private readonly Process process;

        public int ID => process.Id;

        public string Name => process.ProcessName;

        public string NameAndID => Name + "(" + ID + ")";

        public ProcessViewModel(Process process)
        {
            this.process = process;
        }  

        public override int GetHashCode()
        {
            return process.GetHashCode() * 7;
        }
    }
}
