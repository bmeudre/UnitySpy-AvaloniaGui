namespace HackF5.UnitySpy.AvaloniaGui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.AvaloniaGui.Mvvm;
    using HackF5.UnitySpy.AvaloniaGui.View;
    using ReactiveUI;

    public class MainWindowViewModel : ReactiveObject
    {
        private readonly CommandCollection commandCollection;

        private readonly ObservableCollection<ProcessViewModel> processesCollection;

        protected ProcessViewModel selectedProcess;
        
        private readonly MainWindow mainWindow;

        private int linuxModeSelectedIndex = 0;

        private string memPseudoFilePath;

        private string gameExecutableFilePath;        
        
        private AssemblyImageViewModel image;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.processesCollection = new ObservableCollection<ProcessViewModel>();
            this.RefreshProcesses = ReactiveCommand.Create(this.StartRefresh);
            this.OpenMemPseudoFile = ReactiveCommand.Create(this.StartOpenMemPseudoFile);  
            this.OpenGameExecutableFile = ReactiveCommand.Create(this.StartOpenGameExecutableFile);   
            this.BuildImageAssembly = ReactiveCommand.Create(this.StartBuildImageAssembly);
            this.commandCollection = new CommandCollection();  
        }       

        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows); 

        public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);   
        
        public ObservableCollection<ProcessViewModel> Processes => this.processesCollection;

        public ProcessViewModel SelectedProcess 
        {
            get => this.selectedProcess;
            set 
            {
                if (this.selectedProcess != value) 
                {
                    this.RaiseAndSetIfChanged(ref this.selectedProcess, value);
                    if(this.IsWindows)
                    {
                        this.StartBuildImageAssembly();
                    }
                    else if(IsLinux)
                    {
                        this.MemPseudoFilePath = $"/proc/{value.Id}/mem";
                    }
                }
            }
        }

        public string MemPseudoFilePath
        {
            get => this.memPseudoFilePath;
            set => this.RaiseAndSetIfChanged(ref this.memPseudoFilePath, value);
        }
                                
        public string GameExecutableFilePath
        {
            get => this.gameExecutableFilePath;
            set => this.RaiseAndSetIfChanged(ref this.gameExecutableFilePath, value);
        }
                                
        public int LinuxModeSelectedIndex
        {
            get => this.linuxModeSelectedIndex;
            set {
                this.RaiseAndSetIfChanged(ref this.linuxModeSelectedIndex, value); 
                this.RaisePropertyChanged(nameof(IsLinuxDirectMode)); 
            }
        }

        public bool IsLinuxDirectMode => IsLinux && LinuxModeSelectedIndex == 2;

        public AssemblyImageViewModel Image
        {
            get => this.image;
            set => this.RaiseAndSetIfChanged(ref this.image, value);
        }

        public ReactiveCommand<Unit, Unit> RefreshProcesses { get; }
        
        public ReactiveCommand<Unit, Unit> OpenMemPseudoFile { get; }
                
        public ReactiveCommand<Unit, Unit> OpenGameExecutableFile { get; }
        
        public ReactiveCommand<Unit, Unit> BuildImageAssembly { get; }

        private void StartRefresh() 
        {
            Task.Run(this.ExecuteRefreshProcesses);
        }

        private void StartOpenMemPseudoFile() 
        {
            Task.Run(this.ExecuteOpenMemPseudoFileCommand);
        }

        private void StartOpenGameExecutableFile() 
        {
            Task.Run(this.ExecuteOpenGameExecutableFileCommand);
        }

        private void StartBuildImageAssembly() 
        {            
            BuildImageAsync();
            ///Task.Run(() => BuildImageAsync());
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

            this.SelectedProcess = this.Processes.FirstOrDefault(p => p.Name == "MTGA.exe");
        }     
        
        private async void ExecuteOpenMemPseudoFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var filenames = await this.ShowOpenFileDialog("Open /proc/$pid/mem pseudo-file", this.memPseudoFilePath);
			if (filenames != null && filenames.Length > 0)
			{
                this.MemPseudoFilePath = filenames[0];
			}
		}
        
        private async void ExecuteOpenGameExecutableFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var filenames = await this.ShowOpenFileDialog("Open game executable file", this.gameExecutableFilePath);
			if (filenames != null && filenames.Length > 0)
			{
                this.GameExecutableFilePath = filenames[0];
			}
		}

        private async Task<string[]> ShowOpenFileDialog(string tile, string preselectedFile)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = tile;
			dlg.AllowMultiple = false;

            dlg.Directory = Path.GetDirectoryName(preselectedFile);

			//dlg.RestoreDirectory = true;
			return await dlg.ShowAsync(mainWindow);
		}
        
        private void /*async Task*/ BuildImageAsync()
        {

            IAssemblyImage assemblyImage;            
            try
            {           
                ProcessFacade processFacade;
                if(IsWindows)      
                {
                    processFacade = new ProcessFacadeWindows(this.selectedProcess.Id);
                }
                else if(IsOSX)
                {   
                    processFacade = new ProcessFacadeOSX(this.selectedProcess.Id);
                }
                else if(IsLinux)
                {
                    switch(LinuxModeSelectedIndex)
                    {
                        case 0: 
                            processFacade = new ProcessFacadeLinuxClient(this.selectedProcess.Id, this.GameExecutableFilePath);
                            break;
                        case 1: 
                            processFacade = new ProcessFacadeLinuxPTrace(this.selectedProcess.Id, this.GameExecutableFilePath);
                            break;
                        case 2: 
                            processFacade = new ProcessFacadeLinuxDirect(this.selectedProcess.Id, this.MemPseudoFilePath, this.GameExecutableFilePath);
                            break;
                        default: 
                            throw new NotSupportedException("Linux mode not supported");
                    }
                }
                else
                {
                    throw new NotSupportedException("Platform not supported");
                }
                assemblyImage = AssemblyImageFactory.Create(processFacade);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
                //return;
                //await DialogService.ShowAsync(
                    // $"Failed to load process {process.Name} ({process.ProcessId}).",
                    // ex.Message);
            }

            TypeDefinitionContentViewModel.Factory typeDefContFactory = 
                type => new TypeDefinitionContentViewModel(type, 
                    staticField => new StaticFieldViewModel(staticField));

            ManagedObjectInstanceContentViewModel.Factory managedInstanceFactory = 
                instance => new ManagedObjectInstanceContentViewModel(instance);

            ListItemViewModel.Factory listItemFactory = 
                (item, index) => new ListItemViewModel(item, index);

            ListContentViewModel.Factory listContentFactory = 
                list => new ListContentViewModel(list, listItemFactory);

            TypeDefinitionViewModel.Factory typeDefFactory = 
                type => new TypeDefinitionViewModel(this.commandCollection, 
                                                    type, 
                                                    typeDefContFactory, 
                                                    managedInstanceFactory, 
                                                    listContentFactory);

            this.Image = new AssemblyImageViewModel(assemblyImage, typeDefFactory);

            bool hasPapa = false;
            foreach(var type in assemblyImage.TypeDefinitions)
            {
                if(type.Name.StartsWith("PAPA")) 
                {
                    hasPapa = true;
                    break;
                }
            }

            Console.WriteLine($"========= Type Definitions Read = {assemblyImage.TypeDefinitions.Count()} Has PAPA = {hasPapa} =========");
        }    
    }
}
