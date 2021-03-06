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
  using Avalonia.Threading;
  using HackF5.UnitySpy.Detail;
  using HackF5.UnitySpy.Util;
  using HackF5.UnitySpy.AvaloniaGui.Mvvm;
  using HackF5.UnitySpy.AvaloniaGui.View;
  using ReactiveUI;

  public class MainWindowViewModel : ReactiveObject
  {
    private readonly CommandCollection commandCollection;

    private readonly ObservableCollection<ProcessViewModel> processesCollection;

    protected ProcessViewModel selectedProcess;

    private readonly MainWindow mainWindow;

    private RawMemoryView rawMemoryView;

    private int linuxModeSelectedIndex = 0;

    private string memPseudoFilePath;

    private string gameExecutableFilePath;

    private ProcessFacade processFacade;

    private AssemblyImageViewModel image;

    public MainWindowViewModel(MainWindow mainWindow)
    {
      this.mainWindow = mainWindow;
      this.processesCollection = new ObservableCollection<ProcessViewModel>();
      this.RefreshProcesses = ReactiveCommand.Create(this.StartRefresh);
      this.OpenMemPseudoFile = ReactiveCommand.Create(this.StartOpenMemPseudoFile);
      this.OpenGameExecutableFile = ReactiveCommand.Create(this.StartOpenGameExecutableFile);
      this.BuildImageAssembly = ReactiveCommand.Create(this.StartBuildImageAssembly);
      this.ReadRawMemory = ReactiveCommand.Create(this.StartReadRawMemory);
      this.commandCollection = new CommandCollection();
      this.rawMemoryView = new RawMemoryView();
      this.StartRefresh();
    }

    public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public ObservableCollection<ProcessViewModel> Processes => this.processesCollection;

    public ProcessViewModel SelectedProcess
    {
      get => this.selectedProcess;
      set
      {
        if (this.selectedProcess != value)
        {
          this.RaiseAndSetIfChanged(ref this.selectedProcess, value);
          if (this.IsWindows || this.IsMacOS)
          {
            this.StartBuildImageAssembly();
          }
          else if (IsLinux)
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
      set
      {
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

    public ReactiveCommand<Unit, Unit> ReadRawMemory { get; }

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
      Task.Run(() => BuildImageAsync());
    }

    private void StartReadRawMemory()
    {
      Dispatcher.UIThread.InvokeAsync(this.ShowRawMemoryView);
    }

    private async Task ExecuteRefreshProcesses()
    {
      Process[] processes = await Task.Run(Process.GetProcesses);

      this.processesCollection.Clear();
      IEnumerable<Process> orderedProcesses = processes;//.OrderBy(p => p.ProcessName);
      foreach (Process process in orderedProcesses)
      {
        this.processesCollection.Add(new ProcessViewModel(process));
      }

      this.SelectedProcess = this.Processes.FirstOrDefault(p => p.Name == "MTGA.exe" || p.Name == "MTGA");
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

      return await dlg.ShowAsync(mainWindow);
    }

    public async Task ShowRawMemoryView()
    {
      Console.WriteLine("DEBUG: Showing MemoryView");
      //Dispatcher.UIThread.InvokeAsync(() => await MessageBox.Show(mainWindow, "Hello"));

      //await Dispatcher.UIThread.InvokeAsync(() => MessageBox.ShowAsync(mainWindow, "Hello"));
      await MessageBox.ShowAsync(mainWindow, "Hello");

      if (this.processFacade == null)
      {
        this.CreateProcessFacade();
      }
      Console.WriteLine("DEBUG: Showing MemoryView la puta");
      await this.rawMemoryView.ShowDialog(this.mainWindow);
    }

    private async Task BuildImageAsync()
    {
      IAssemblyImage assemblyImage;
      try
      {
        MonoLibraryOffsets monoLibraryOffsets;
        if (this.processFacade == null)
        {
          this.CreateProcessFacade();
        }

        if (this.IsWindows)
        {
          ProcessFacadeWindows windowsProcessFacade = this.processFacade as ProcessFacadeWindows;
          monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(windowsProcessFacade.GetMainModuleFileName());
        }
        else if (this.IsMacOS)
        {
          ProcessFacadeMacOS macOSProcessFacade = this.processFacade as ProcessFacadeMacOS;
          monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(macOSProcessFacade.Process.MainModule.FileName); ;
        }
        else if (this.IsLinux)
        {
          monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(gameExecutableFilePath);
        }
        else
        {
          throw new NotSupportedException("Platform not supported");
        }
        UnityProcessFacade unityProcess = new UnityProcessFacade(processFacade, monoLibraryOffsets);
        assemblyImage = AssemblyImageFactory.Create(unityProcess);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        await MessageBox.ShowAsync(
            $"Failed to load process {this.selectedProcess.Name} ({this.selectedProcess.Id}).",
            ex.Message);
        return;
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
      foreach (var type in assemblyImage.TypeDefinitions)
      {
        if (type.Name.StartsWith("PAPA"))
        {
          hasPapa = true;
          break;
        }
      }

      Console.WriteLine($"========= Type Definitions Read = {assemblyImage.TypeDefinitions.Count()} Has PAPA = {hasPapa} =========");
    }

    private void CreateProcessFacade()
    {
      if (this.IsLinux)
      {
        switch (this.LinuxModeSelectedIndex)
        {
          case 0:
            this.processFacade = new ProcessFacadeLinuxClient(this.selectedProcess.Id, this.GameExecutableFilePath);
            break;
          case 1:
            this.processFacade = new ProcessFacadeLinuxPTrace(this.selectedProcess.Id, this.GameExecutableFilePath);
            break;
          case 2:
            this.processFacade = new ProcessFacadeLinuxDirect(this.selectedProcess.Id, this.MemPseudoFilePath, this.GameExecutableFilePath);
            break;
          default:
            throw new NotSupportedException("Linux mode not supported");
        }
      }
      else
      {
        Process process = Process.GetProcessById(this.selectedProcess.Id);
        if (this.IsWindows)
        {
          this.processFacade = new ProcessFacadeWindows(process);
        }
        else if (this.IsMacOS)
        {
          this.processFacade = new ProcessFacadeMacOS(process);
        }
        else
        {
          throw new NotSupportedException("Platform not supported");
        }
      }
      Dispatcher.UIThread.InvokeAsync(() =>
          ((RawMemoryViewModel)this.rawMemoryView.DataContext).Process = this.processFacade);
    }
  }
}
