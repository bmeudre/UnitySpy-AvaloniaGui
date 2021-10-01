namespace HackF5.UnitySpy.AvaloniaGui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reactive;
    using System.Threading.Tasks;
    using ReactiveUI;

    public class WindowsProcessSourceViewModel : ReactiveObject
    {
        private readonly ObservableCollection<ProcessViewModel> processesCollection;

        public ProcessViewModel selectedProcess;
        
        public WindowsProcessSourceViewModel()
        {
            this.processesCollection = new ObservableCollection<ProcessViewModel>();
            this.RefreshProcesses = ReactiveCommand.Create(StartRefresh);
        }  
        
        public ObservableCollection<ProcessViewModel> Processes => this.processesCollection;

        public ProcessViewModel SelectedProcess 
        {
            get => this.selectedProcess;
            set 
            {
                if (this.selectedProcess != value) 
                {
                    this.selectedProcess = value;
                    Task.Run(() => this.BuildImageAsync(this.selectedProcess));
                }
            }
        }

        public ReactiveCommand<Unit, Unit> RefreshProcesses { get; }

        private void StartRefresh() 
        {
            Task.Run(ExecuteRefreshProcesses);
        }

        private async Task ExecuteRefreshProcesses()
        {
            Process[] processes = await Task.Run(Process.GetProcesses);

            this.processesCollection.Clear();
            IEnumerable<Process> orderedProcesses = processes;//.OrderBy(p => p.ProcessName);
            foreach(Process process in orderedProcesses) 
            {
                this.processesCollection.Add(new ProcessViewModel(process));
            }
        } 
        
        private void/*async Task*/ BuildImageAsync(ProcessViewModel process)
        {
            // try
            // {
            //     this.Image = this.imageFactory(AssemblyImageFactory.Create(process.ProcessId));
            // }
            // catch (Exception ex)
            // {
            //     await DialogService.ShowAsync(
            //         $"Failed to load process {process.Name} ({process.ProcessId}).",
            //         ex.Message);
            // }

            try
            {
                IAssemblyImage assemblyImage = AssemblyImageFactory.Create(process.ID);
            }
            catch (Exception ex)
            {
                return;
                //await DialogService.ShowAsync(
                    // $"Failed to load process {process.Name} ({process.ProcessId}).",
                    // ex.Message);
            }
        }


    }
}
