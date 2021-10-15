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
        public static IAssemblyImage Create(UnityProcessFacade process, string assemblyName = "Assembly-CSharp")
        {
            var monoModule = process.GetMonoModule();
            var moduleDump = process.ReadModule(monoModule);
            var rootDomainFunctionAddress = AssemblyImageFactory.GetRootDomainFunctionAddress(moduleDump, monoModule, process.Is64Bits);

            return AssemblyImageFactory.GetAssemblyImage(process, assemblyName, rootDomainFunctionAddress);
        }

        private static AssemblyImage GetAssemblyImage(UnityProcessFacade process, string name, IntPtr rootDomainFunctionAddress)
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