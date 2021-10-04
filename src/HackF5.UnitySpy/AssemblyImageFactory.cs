namespace HackF5.UnitySpy
{
    using System;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// A factory that creates <see cref="IAssemblyImage"/> instances that provides access into a Unity application's
    /// managed memory.
    /// SEE: https://github.com/Unity-Technologies/mono.
    /// </summary>
    [PublicAPI]
    public static class AssemblyImageFactory
    {
        /// <summary>
        /// Creates an <see cref="IAssemblyImage"/> that provides access into a Unity application's managed memory on Windows.
        /// </summary>
        /// <param name="processId">
        /// The id of the Unity process to be inspected.
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly to be inspected. The default setting of 'Assembly-CSharp' is probably what you want.
        /// </param>
        /// <returns>
        /// An <see cref="IAssemblyImage"/> that provides access into a Unity application's managed memory.
        /// </returns>
        public static IAssemblyImage Create(int processId, string assemblyName = "Assembly-CSharp")
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new InvalidOperationException(
                    "This library reads data directly from a process's memory, so is platform specific "
                    + "and only runs under Windows. It might be possible to get it running under macOS, but...");
            }
            
            return Create(new ProcessFacadeWindows(processId), assemblyName);
        }

        /// <summary>
        /// Creates an <see cref="IAssemblyImage"/> that provides access into
        /// a Unity application's managed memory on Linux through the /proc/$pid/mem file. It needs root access.
        /// </summary>
        /// <param name="memPseudoFile">
        /// /proc/$pid/mem file (needs root access)
        /// </param>
        /// <param name="mapsPseudoFile">
        /// /proc/$pid/maps file
        /// </param>
        /// <param name="gameExecutableFile">
        /// The location of the game's main executable
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly to be inspected. The default setting of 'Assembly-CSharp' is probably what you want.
        /// </param>
        /// <returns>
        /// An <see cref="IAssemblyImage"/> that provides access into a Unity application's managed memory.
        /// </returns>
        public static IAssemblyImage Create(string memPseudoFile, string mapsPseudoFile, string gameExecutableFile, string assemblyName = "Assembly-CSharp")
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new InvalidOperationException(
                    "This library reads data directly from a process's memory, so is platform specific "
                    + " and only runs under unix. It might be possible to get it running under windows, but...");
            }
            
            return Create(new ProcessFacadeLinuxDirect(memPseudoFile, mapsPseudoFile, gameExecutableFile), assemblyName);
        }

        /// <summary>
        /// Creates an <see cref="IAssemblyImage"/> that provides access into
        /// a Unity application's managed memory on Linux through client-server model
        /// </summary>
        /// <param name="mapsPseudoFile">
        /// /proc/$pid/maps file
        /// </param>
        /// <param name="gameExecutableFile">
        /// The location of the game's main executable
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly to be inspected. The default setting of 'Assembly-CSharp' is probably what you want.
        /// </param>
        /// <returns>
        /// An <see cref="IAssemblyImage"/> that provides access into a Unity application's managed memory.
        /// </returns>
        public static IAssemblyImage Create(string mapsPseudoFile, string gameExecutableFile, string assemblyName = "Assembly-CSharp")
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new InvalidOperationException(
                    "This library reads data directly from a process's memory, so is platform specific "
                    + " and only runs under unix. It might be possible to get it running under windows, but...");
            }
            
            return Create(new ProcessFacadeLinuxClient(mapsPseudoFile, gameExecutableFile), assemblyName);
        }
        
        public static IAssemblyImage Create(ProcessFacade process, string assemblyName = "Assembly-CSharp")
        {
            var monoModule = process.GetMonoModule();
            var moduleDump = process.ReadModule(monoModule);
            var rootDomainFunctionAddress = AssemblyImageFactory.GetRootDomainFunctionAddress(moduleDump, monoModule, process.Is64Bits);

            return AssemblyImageFactory.GetAssemblyImage(process, assemblyName, rootDomainFunctionAddress);
        }

        private static AssemblyImage GetAssemblyImage(ProcessFacade process, string name, IntPtr rootDomainFunctionAddress)
        {
            IntPtr domain;
            if (process.Is64Bits)
            {
                // Offsets taken by decompiling the 64 bits version of mono-2.0-bdwgc.dll
                //
                // mov rax, [rip + 0x46ad39]
                // ret
                //
                // These two lines in Hex translate to
                // 488B05 39AD46 00
                // C3
                // 
                // So wee need to offset the first three bytes to get to the relative offset we need to add to rip
                // rootDomainFunctionAddress + 3
                //
                // rip has the current value of the rootDoaminAddress plus the 7 bytes of the first instruction (mov rax, [rip + 0x46ad39])
                // then we need to add this offsets to get the domain starting address
                var offset = process.ReadInt32(rootDomainFunctionAddress + 3) + 7;
                //// pointer to struct of type _MonoDomain
                domain = process.ReadPtr(rootDomainFunctionAddress + offset);
            } 
            else
            {
                var domainAddress = process.ReadPtr(rootDomainFunctionAddress + 1);
                //// pointer to struct of type _MonoDomain
                domain = process.ReadPtr(domainAddress);
            }

            //// pointer to array of structs of type _MonoAssembly
            var assemblyArrayAddress = process.ReadPtr(domain + process.MonoLibraryOffsets.ReferencedAssemblies);
            for (var assemblyAddress = assemblyArrayAddress;
                assemblyAddress != Constants.NullPtr;
                assemblyAddress = process.ReadPtr(assemblyAddress + process.SizeOfPtr))
            {
                var assembly = process.ReadPtr(assemblyAddress);
                var assemblyNameAddress = process.ReadPtr(assembly + (process.SizeOfPtr * 2));
                var assemblyName = process.ReadAsciiString(assemblyNameAddress);
                if (assemblyName == name)
                {
                    return new AssemblyImage(process, process.ReadPtr(assembly + process.MonoLibraryOffsets.AssemblyImage));
                }
            }

            throw new InvalidOperationException($"Unable to find assembly '{name}'");
        }        

        private static IntPtr GetRootDomainFunctionAddress(byte[] moduleDump, ModuleInfo monoModuleInfo, bool is64Bits)
        {
            // offsets taken from https://docs.microsoft.com/en-us/windows/desktop/Debug/pe-format
            // ReSharper disable once CommentTypo
            var startIndex = moduleDump.ToInt32(PEFormatOffsets.Signature); // lfanew

            var exportDirectoryIndex = startIndex + PEFormatOffsets.GetExportDirectoryIndex(is64Bits);
            var exportDirectory = moduleDump.ToInt32(exportDirectoryIndex);

            var numberOfFunctions = moduleDump.ToInt32(exportDirectory + PEFormatOffsets.NumberOfFunctions);
            var functionAddressArrayIndex = moduleDump.ToInt32(exportDirectory + PEFormatOffsets.FunctionAddressArrayIndex);
            var functionNameArrayIndex = moduleDump.ToInt32(exportDirectory + PEFormatOffsets.FunctionNameArrayIndex);

            var rootDomainFunctionAddress = Constants.NullPtr;
            for (var functionIndex = 0;
                functionIndex < (numberOfFunctions * PEFormatOffsets.FunctionEntrySize);
                functionIndex += PEFormatOffsets.FunctionEntrySize)
            {
                var functionNameIndex = moduleDump.ToInt32(functionNameArrayIndex + functionIndex);
                var functionName = moduleDump.ToAsciiString(functionNameIndex);
                if (functionName == "mono_get_root_domain")
                {
                    rootDomainFunctionAddress = monoModuleInfo.BaseAddress
                        + moduleDump.ToInt32(functionAddressArrayIndex + functionIndex);

                    break;
                }
            }

            if (rootDomainFunctionAddress == Constants.NullPtr)
            {
                throw new InvalidOperationException("Failed to find mono_get_root_domain function.");
            }

            return rootDomainFunctionAddress;
        }
    }
}