namespace HackF5.UnitySpy.AvaloniaGui.ViewModels
{
    using System;
    using System.IO;
    using System.Reactive;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using HackF5.UnitySpy.AvaloniaGui.Views;
    using ReactiveUI;

    public class LinuxProcessSourceViewModel : ReactiveObject
    {
        private readonly MainWindow mainWindow;

        private string memPseudoFilePath;

        private string mapsPseudoFilePath;

        private string gameExecutableFilePath;
        
        public LinuxProcessSourceViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            OpenMemPseudoFile = ReactiveCommand.Create(this.StartOpenMemPseudoFile);  
            OpenMapsPseudoFile = ReactiveCommand.Create(this.StartOpenMapsPseudoFile);  
            OpenGameExecutableFile = ReactiveCommand.Create(this.StartOpenGameExecutableFile);   
            BuildImageAssembly = ReactiveCommand.Create(this.StartBuildImageAssembly);          
        }  
        
        public string MemPseudoFilePath
        {
            get => this.memPseudoFilePath;
            set => this.RaiseAndSetIfChanged(ref this.memPseudoFilePath, value);
        }
        
        public string MapsPseudoFilePath
        {
            get => this.mapsPseudoFilePath;
            set => this.RaiseAndSetIfChanged(ref this.mapsPseudoFilePath, value);
        }
                                
        public string GameExecutableFilePath
        {
            get => this.gameExecutableFilePath;
            set => this.RaiseAndSetIfChanged(ref this.gameExecutableFilePath, value);
        }
        
        public ReactiveCommand<Unit, Unit> OpenMemPseudoFile { get; }
        
        public ReactiveCommand<Unit, Unit> OpenMapsPseudoFile { get; }
        
        public ReactiveCommand<Unit, Unit> OpenGameExecutableFile { get; }
        
        public ReactiveCommand<Unit, Unit> BuildImageAssembly { get; }

        private void StartOpenMemPseudoFile() 
        {
            Task.Run(this.ExecuteOpenMemPseudoFileCommand);
        }

        private void StartOpenMapsPseudoFile() 
        {
            Task.Run(this.ExecuteOpenMapsPseudoFileCommand);
        }

        private void StartOpenGameExecutableFile() 
        {
            Task.Run(this.ExecuteOpenGameExecutableFileCommand);
        }

        private void StartBuildImageAssembly() 
        {            
            Task.Run(() => this.BuildImageAsync());
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
        
        private async void ExecuteOpenMapsPseudoFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var filenames = await this.ShowOpenFileDialog("Open /proc/$pid/maps pseudo-file", this.mapsPseudoFilePath);
			if (filenames != null && filenames.Length > 0)
			{
                this.MapsPseudoFilePath = filenames[0];
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
        
        private async Task BuildImageAsync()
        {

            ulong byteCount = 0;

            string[] lineColumnValues;
            string[] memoryRegion;
            foreach(string line in File.ReadAllLines(MapsPseudoFilePath)) 
            {
                lineColumnValues = line.Split(' ');
                memoryRegion = lineColumnValues[0].Split('-');
                byteCount += Convert.ToUInt64(memoryRegion[1], 16) - Convert.ToUInt64(memoryRegion[0], 16);
            }

            Console.WriteLine($"Bytes in memory = {byteCount}");


            // try
            // {
            //     IAssemblyImage assemblyImage = AssemblyImageFactory.Create(MemPseudoFilePath, MapsPseudoFilePath, GameExecutableFilePath);
            // }
            // catch (Exception ex)
            // {
            //     return;
            //     //await DialogService.ShowAsync(
            //         // $"Failed to load process {process.Name} ({process.ProcessId}).",
            //         // ex.Message);
            // }
        }


    }
}
