﻿namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public abstract class ProcessFacadeLinux : ProcessFacade
    {        
        private readonly List<MemoryMapping> mappings;
        
        public ProcessFacadeLinux(string mapsFilePath, string gameExecutableFilePath)
        {
            this.monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(gameExecutableFilePath);

            string[] mappingsInFile = File.ReadAllLines(mapsFilePath);
            this.mappings = new List<MemoryMapping>(mappingsInFile.Length);
            string[] lineColumnValues;
            string[] memoryRegion;
            string name;
            foreach (var line in mappingsInFile) {
                lineColumnValues = line.Split(' ');
                memoryRegion = lineColumnValues[0].Split('-');
                if (lineColumnValues.Length > 6)
                {
                    name = line.Substring(73);
                }   
                else
                {
                    name = "";
                }
                mappings.Add(new MemoryMapping(memoryRegion[0], memoryRegion[1], name, lineColumnValues[4] != "0"));
            }
        }
        
        protected override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            int length = size ?? buffer.Length;
            if(mappings.Exists(mapping => mapping.Contains(processAddress)))
            {
                ReadProcessMemory(buffer, processAddress, length);
            }
            else
            {
                Console.Error.WriteLine($"Attempting to read unmapped address {processAddress.ToString("X")} + {length}"); 
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }  
            }
        }

        protected abstract void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int size);

        public override ModuleInfo GetMonoModule() 
        {            
            int mappingIndex = mappings.FindIndex(mapping => 
                mapping.ModuleName.EndsWith(this.monoLibraryOffsets.MonoLibraryName));
            
            if (mappingIndex < 0)
            {
                throw new Exception("Mono module not found");
            }

            IntPtr startingAddress = mappings[mappingIndex].StartAddress;
            string moduleName = mappings[mappingIndex].ModuleName;

            while(!mappings[mappingIndex].IsStartingModule || mappings[mappingIndex].ModuleName == moduleName)
            {
                mappingIndex++;
            }        

            uint size = Convert.ToUInt32(MemoryMapping.GetSize(startingAddress, mappings[mappingIndex].EndAddress));
            
            Console.WriteLine($"Mono Module starting address = {startingAddress.ToString("X")}, end address = {mappings[mappingIndex].EndAddress}");

            return new ModuleInfo(this.monoLibraryOffsets.MonoLibraryName, startingAddress, size);
        }

        protected struct MemoryMapping
        {
            public IntPtr StartAddress;

            public IntPtr EndAddress;

            public string ModuleName;

            public bool IsStartingModule;

            public long Size => MemoryMapping.GetSize(StartAddress, EndAddress);

            public MemoryMapping(string startAddress, string endAddress, string moduleName, bool isStartingModule)
                : this(new IntPtr(Convert.ToInt64(startAddress, 16)),
                       new IntPtr(Convert.ToInt64(endAddress, 16)),
                       moduleName,
                       isStartingModule
                ) 
            {

            }

            public MemoryMapping(IntPtr startAddress, IntPtr endAddress, string moduleName, bool isStartingModule)
            {
                this.StartAddress = startAddress;
                this.EndAddress = endAddress;
                this.ModuleName = moduleName;
                this.IsStartingModule = isStartingModule;
            }

            public bool Contains(IntPtr address, int length = 0)
            {                
                long addressAsLong = address.ToInt64();
                return addressAsLong >= StartAddress.ToInt64() && addressAsLong + length < EndAddress.ToInt64();
            }

            public static long GetSize(IntPtr startAddress, IntPtr endAddress) 
                => endAddress.ToInt64() - startAddress.ToInt64();
        }
    }
}