namespace HackF5.UnitySpy.Detail
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// A facade over an unity process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class UnityProcessFacade
    {
        private readonly ProcessFacade process;

        private readonly MonoLibraryOffsets monoLibraryOffsets;

        public UnityProcessFacade(ProcessFacade process, MonoLibraryOffsets monoLibraryOffsets)
        {
            this.process = process;
            this.monoLibraryOffsets = monoLibraryOffsets;
            this.process.Is64Bits = monoLibraryOffsets.Is64Bits;
        }

        public MonoLibraryOffsets MonoLibraryOffsets => this.monoLibraryOffsets;

        public bool Is64Bits => this.process.Is64Bits;

        public int SizeOfPtr => this.process.SizeOfPtr;

        public ProcessFacade Process => process;

        public string ReadAsciiString(IntPtr address, int maxSize = 1024) => 
            process.ReadAsciiString(address, maxSize);

        public string ReadAsciiStringPtr(IntPtr address, int maxSize = 1024) =>
            this.ReadAsciiString(this.ReadPtr(address), maxSize);

        public int ReadInt32(IntPtr address) => this.process.ReadInt32(address);

        public long ReadInt64(IntPtr address) => this.process.ReadInt64(address);

        public object ReadManaged([NotNull] TypeInfo type, IntPtr address)
            => this.process.ReadManaged(type, address);

        public IntPtr ReadPtr(IntPtr address) => this.process.ReadPtr(address);

        public uint ReadUInt32(IntPtr address) => this.process.ReadUInt32(address);

        public ulong ReadUInt64(IntPtr address) => this.process.ReadUInt64(address);

        public byte ReadByte(IntPtr address) => this.process.ReadByte(address);
        
        public byte[] ReadModule([NotNull] ModuleInfo moduleInfo) => this.process.ReadModule(moduleInfo);

        public ModuleInfo GetMonoModule() =>
            this.process.GetModule(this.monoLibraryOffsets.MonoLibraryName);
    }
}