namespace HackF5.UnitySpy.Detail
{
  using System;
  using System.Diagnostics;
  using System.Runtime.InteropServices;
  using JetBrains.Annotations;
  using System.IO;

  /// <summary>
  /// A MacOS specific facade over a process that provides access to its memory space.
  /// </summary>
  [PublicAPI]
  public class ProcessFacadeMacOS : ProcessFacade
  {
    protected readonly Process process;

    public ProcessFacadeMacOS(Process process)
    {
      this.process = process;
    }

    public Process Process => this.process;

    public override void ReadProcessMemory(
        byte[] buffer,
        IntPtr processAddress,
        bool allowPartialRead = false,
        int? size = default)
    {
      var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

      try
      {
        var bufferPointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
        if (0 != ProcessFacadeMacOS.ReadProcessMemory(
            this.process.Id,
            processAddress,
            bufferPointer,
            size ?? buffer.Length))
        {
          var error = Marshal.GetLastWin32Error();
          if ((error == 299) && allowPartialRead)
          {
            return;
          }

          throw new InvalidOperationException($"Could not read memory. Error code: {error}");
        }
      }
      finally
      {
        bufferHandle.Free();
      }
    }


    public override ModuleInfo GetModule(string moduleName)
    {
      if (!this.Is64Bits)
      {
        throw new NotSupportedException("MacOS for 32 binaries is not supported currently");
      }

      uint size = 0;
      IntPtr address = GetModuleInfo(this.process.Id, moduleName, ref size);

      // Module not found, il2cpp is enabled, we need to inject mono lib first
      if (address.ToInt32() == 0)
      {
        Console.WriteLine("DEBUG: IL2CPP IS ENABLED");
        byte[] monoLib = File.ReadAllBytes("libmonobdwgc-2.0.dylib");

        Console.WriteLine($"DEBUG: libmonobdwgc-2.0.dylib size = {monoLib.Length}");
        throw new Exception("IL2CPP not supported yet");
      }

      Console.WriteLine($"DEBUG: GetModuleInfo address = {address}");

      return new ModuleInfo(moduleName, address, size);
    }

    [DllImport("macos.dylib", EntryPoint = "read_process_memory_to_buffer", SetLastError = true)]
    private static extern int ReadProcessMemory(
        int processId,
        IntPtr lpBaseAddress,
        IntPtr lpBuffer,
        int nSize);

    [DllImport("macos.dylib", EntryPoint = "get_module_info", SetLastError = true)]
    private static extern IntPtr GetModuleInfo(
        int processId,
        string moduleName,
        ref uint nSize);
  }
}