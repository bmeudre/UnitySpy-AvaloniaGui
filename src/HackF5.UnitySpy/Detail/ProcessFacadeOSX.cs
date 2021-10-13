namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeOSX : ProcessFacade
    {        
        protected readonly Process process;

        private readonly ProcessFacadeClient client;
        
        public ProcessFacadeOSX(int processId)
        {            
            this.process = Process.GetProcessById(processId);
            this.client = new ProcessFacadeClient();
            this.monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(this.process.MainModule.FileName);
        }
                
        protected override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                var bufferPointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                if (0 != ProcessFacadeOSX.ReadProcessMemory(
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

        public override ModuleInfo GetMonoModule()
        {          
            if (!this.monoLibraryOffsets.Is64Bits)
            {
                throw new NotSupportedException("MacOS for 32 binaries is not supported currently");
            }

            foreach (ProcessModule module in this.process.Modules)
            {
                if (module.ModuleName == this.monoLibraryOffsets.MonoLibraryName)
                {
                    uint memorySize = Convert.ToUInt32(module.ModuleMemorySize);
                    return new ModuleInfo(module.ModuleName, module.BaseAddress, memorySize);
                }
            }

            throw new InvalidOperationException("Mono module not found. ");
        }

        [DllImport("/tmp/read_mem.dylib", SetLastError = true)]
        private static extern int ReadProcessMemory(
            int processId,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize);
    }
}