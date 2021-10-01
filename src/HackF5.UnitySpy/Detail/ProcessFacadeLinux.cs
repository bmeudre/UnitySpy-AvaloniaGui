﻿namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinux : ProcessFacade
    {
        private readonly string memFilePath;
        private readonly string mapsFilePath;
        
        public ProcessFacadeLinux(string memFilePath, string mapsFilePath, string gameExecutableFilePath)
        {
            this.memFilePath = memFilePath;
            this.mapsFilePath = mapsFilePath;
            this.monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(gameExecutableFilePath);
        }

        protected override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            int length = size ?? buffer.Length;
            using(FileStream memFileStream = new FileStream(memFilePath, FileMode.Create))
            {
                // Write the data to the file, byte by byte.
                for(int i = 0; i < length; i++)
                {
                    if(buffer[i] != memFileStream.ReadByte())
                    {
                        Console.WriteLine("Error reading data.");
                        return;
                    }
                }
            }
        }

        public override ModuleInfo GetMonoModule() 
        {            
            var lines = File.ReadLines(mapsFilePath).GetEnumerator();
            while(lines.MoveNext()) 
            {
                if (lines.Current.EndsWith(this.monoLibraryOffsets.MonoLibraryName)) 
                {
                    var lineColumnValues = lines.Current.Split(' ');
                    var memoryRegion = lineColumnValues[0].Split('-');
                    var baseAddressStr = memoryRegion[0];
                    var baseAddress = ParsePtr(baseAddressStr);
                    while(lines.MoveNext()) 
                    {
                        lineColumnValues = lines.Current.Split(' ');
                        if (lineColumnValues[4] != "0")
                        {
                            break;
                        }
                        memoryRegion = lineColumnValues[0].Split('-');
                    }
                    uint memorySize = GetMemorySize(baseAddressStr, memoryRegion[1]);
                    return new ModuleInfo(this.monoLibraryOffsets.MonoLibraryName, baseAddress, memorySize);
                }
            }
            throw new Exception("Mono module not found");
        }

        private IntPtr ParsePtr(string hexString) 
        {
            if (this.monoLibraryOffsets.Is64Bits)
            {
                return new IntPtr(Convert.ToInt64(hexString, 16));
            }
            else
            {
                return new IntPtr(Convert.ToInt32(hexString, 16));
            }
        }

        private uint GetMemorySize(string startAddress, string endAddress)
        {
            if (this.monoLibraryOffsets.Is64Bits)
            {
                ulong size = Convert.ToUInt64(endAddress, 16) - Convert.ToUInt64(startAddress, 16);
                return Convert.ToUInt32(size);
            }
            else
            {
                return Convert.ToUInt32(endAddress, 16) - Convert.ToUInt32(startAddress, 16);
            }
        }
    }
}