namespace HackF5.UnitySpy.AvaloniaGui.ViewModels
{
    using System;
    using System.Linq;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reactive;

    using ReactiveUI;
    using Avalonia.Controls;

    using HackF5.UnitySpy;
    
    public class MainWindowViewModel : ReactiveObject
    {
        private ObservableCollection<ProcessViewModel> processesCollection;

        public ObservableCollection<ProcessViewModel> Processes => processesCollection;

        public ProcessViewModel selectedProcess;
        public ProcessViewModel SelectedProcess {
            get => selectedProcess;
            set {
                if(selectedProcess != value) {
                    selectedProcess = value;
                    Task.Run(() => BuildImageAsync(selectedProcess));
                }
            }
        }
        
        public ReactiveCommand<Unit, Unit> RefreshProcesses { get; }

        public MainWindowViewModel()
        {
            processesCollection = new ObservableCollection<ProcessViewModel>();
            RefreshProcesses = ReactiveCommand.Create(StartRefresh);
        }       

        private void StartRefresh() 
        {
            Task.Run(ExecuteRefreshProcesses);
        }

        private async Task ExecuteRefreshProcesses()
        {
            Process[] processes = await Task.Run(Process.GetProcesses);

            processesCollection.Clear();
            IEnumerable<Process> orderedProcesses = processes;//.OrderBy(p => p.ProcessName);
            foreach(Process process in orderedProcesses) 
            {
                processesCollection.Add(new ProcessViewModel(process));
            }

            //this.SelectedProcess = this.Processes.FirstOrDefault(p => p.Name == "Hearthstone");
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
