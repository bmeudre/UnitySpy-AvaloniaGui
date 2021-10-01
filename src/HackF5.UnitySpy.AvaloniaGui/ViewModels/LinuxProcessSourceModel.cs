namespace HackF5.UnitySpy.AvaloniaGui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using HackF5.UnitySpy.AvaloniaGui.Views;
    using ReactiveUI;

    public class LinuxProcessSourceViewModel : ReactiveObject
    {
        private readonly MainWindow mainWindow;
        
        public LinuxProcessSourceViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            OpenMemPseudoFile = ReactiveCommand.Create(this.StartOpenMemPseudoFile);  
            OpenMapsPseudoFile = ReactiveCommand.Create(this.StartOpenMapsPseudoFile);  
            OpenGameExecutableFile = ReactiveCommand.Create(this.StartOpenGameExecutableFile);   
            BuildImageAssembly = ReactiveCommand.Create(this.StartOpenGameExecutableFile);          
        }  
        
        public string MemPseudoFilePath { get; set; }
        
        public string MapsPseudoFilePath { get; set; }
        
        public string GameExecutableFilePath { get; set; }
        
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
			var filenames = await this.ShowOpenFileDialog("Open /proc/$pid/mem pseudo-file");
			if (filenames != null && filenames.Length > 1)
			{

			}
		}
        
        private async void ExecuteOpenMapsPseudoFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var filenames = await this.ShowOpenFileDialog("Open /proc/$pid/maps pseudo-file");
			if (filenames != null && filenames.Length > 1)
			{

			}
		}
        
        private async void ExecuteOpenGameExecutableFileCommand()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var filenames = await this.ShowOpenFileDialog("Open game executable file");
			if (filenames != null && filenames.Length > 1)
			{

			}
		}

        private async Task<string[]> ShowOpenFileDialog(string tile, List<FileDialogFilter> filters = null)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = tile;
			dlg.Filters = filters;
			dlg.AllowMultiple = false;
			//dlg.RestoreDirectory = true;
			return await dlg.ShowAsync(mainWindow);
		}
        
        private async Task BuildImageAsync()
        {
            try
            {
                IAssemblyImage assemblyImage = AssemblyImageFactory.Create(MemPseudoFilePath, MapsPseudoFilePath, GameExecutableFilePath);
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
