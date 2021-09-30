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
    using HackF5.UnitySpy.AvaloniaGui.Views;

    public class MainWindowViewModel : ReactiveObject
    {
        private readonly MainWindow mainWindow;

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
        public ReactiveCommand<Unit, Unit> OpenMemPseudoFile { get; }

        public MainWindowViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            processesCollection = new ObservableCollection<ProcessViewModel>();
            RefreshProcesses = ReactiveCommand.Create(StartRefresh);
            OpenMemPseudoFile = ReactiveCommand.Create(StartOpenMemPseudoFile);
        }       

        private void StartRefresh() 
        {
            Task.Run(ExecuteRefreshProcesses);
        }

        private void StartOpenMemPseudoFile() 
        {
            Task.Run(ExecuteOpenMemPseudoFileCommand);
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

        private async void ExecuteOpenMemPseudoFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open /proc/$pid/mem pseudo-file";
			// dlg.Filters = new List<FileDialogFilter>()
			// {
			// 	new FileDialogFilter() { Name = ".NET assemblies", Extensions = {"dll","exe", "winmd" }},
			// 	new FileDialogFilter() { Name = "Nuget Packages (*.nupkg)", Extensions = { "nupkg" }},
			// 	new FileDialogFilter() { Name = "All files", Extensions = { "*" }},
			// };
			dlg.AllowMultiple = false;
			//dlg.RestoreDirectory = true;
			var filenames = await dlg.ShowAsync(mainWindow);
			if (filenames != null && filenames.Length > 0)
			{
                // TODO implement
				// OpenMemPseudoFile(filenames);
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
