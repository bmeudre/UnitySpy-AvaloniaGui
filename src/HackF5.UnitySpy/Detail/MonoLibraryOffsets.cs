// ReSharper disable IdentifierTypo
namespace HackF5.UnitySpy.Detail
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Runtime.InteropServices;
  using System.IO;

  public class MonoLibraryOffsets
  {
    public static readonly MonoLibraryOffsets Unity2018_4_10_x86_Offsets = new MonoLibraryOffsets
    {
      UnityVersion = new UnityVersion(2018, 4, 10),
      Is64Bits = false,
      MonoLibraryName = "libmonobdwgc-2.0.dylib",

      AssemblyImage = 0x44,
      ReferencedAssemblies = 0x6c,
      ImageClassCache = 0x354,
      HashTableSize = 0xc,
      HashTableTable = 0x14,

      TypeDefinitionFieldSize = 0x10,
      TypeDefinitionBitFields = 0x14,
      TypeDefinitionClassKind = 0x1e,
      TypeDefinitionParent = 0x20,
      TypeDefinitionNestedIn = 0x24,
      TypeDefinitionName = 0x2c,
      TypeDefinitionNamespace = 0x30,
      TypeDefinitionVTableSize = 0x38,
      TypeDefinitionSize = 0x5c,
      TypeDefinitionFields = 0x60,
      TypeDefinitionByValArg = 0x74,
      TypeDefinitionRuntimeInfo = 0x84,

      TypeDefinitionFieldCount = 0xa4,
      TypeDefinitionNextClassCache = 0xa8,

      TypeDefinitionGenericContainer = 0x94,

      TypeDefinitionRuntimeInfoDomainVTables = 0x4,

      VTable = 0x28
    };

    public static readonly MonoLibraryOffsets Unity2019_4_5_x64_Offsets = new MonoLibraryOffsets
    {
      UnityVersion = new UnityVersion(2019, 4, 5),
      Is64Bits = true,
      MonoLibraryName = "libmonobdwgc-2.0.dylib",

      AssemblyImage = 0x44 + 0x1c,
      ReferencedAssemblies = 0x6c + 0x5c,
      ImageClassCache = 0x354 + 0x16c,
      HashTableSize = 0xc + 0xc,
      HashTableTable = 0x14 + 0xc,

      TypeDefinitionFieldSize = 0x10 + 0x10,
      TypeDefinitionBitFields = 0x14 + 0xc,
      TypeDefinitionClassKind = 0x1e + 0xc,
      TypeDefinitionParent = 0x20 + 0x10,                         // 0x30
      TypeDefinitionNestedIn = 0x24 + 0x14,                       // 0x38
      TypeDefinitionName = 0x2c + 0x1c,                           // 0x48
      TypeDefinitionNamespace = 0x30 + 0x20,                      // 0x50
      TypeDefinitionVTableSize = 0x38 + 0x24,
      TypeDefinitionSize = 0x5c + 0x20 + 0x18 - 0x4,              // 0x90 Array Element Count
      TypeDefinitionFields = 0x60 + 0x20 + 0x18,                  // 0x98
      TypeDefinitionByValArg = 0x74 + 0x44,
      TypeDefinitionRuntimeInfo = 0x84 + 0x34 + 0x18,             // 0xB8

      TypeDefinitionFieldCount = 0xa4 + 0x34 + 0x10 + 0x18,
      TypeDefinitionNextClassCache = 0xa8 + 0x34 + 0x10 + 0x18 + 0x4,

      TypeDefinitionGenericContainer = 0x94 + 0x34 + 0x18 + 0x10,

      TypeDefinitionRuntimeInfoDomainVTables = 0x4 + 0x4,

      VTable = 0x28 + 0x18
    };

    private static readonly List<MonoLibraryOffsets> SupportedVersions = new List<MonoLibraryOffsets>()
        {
            Unity2018_4_10_x86_Offsets,
            Unity2019_4_5_x64_Offsets
        };

    public UnityVersion UnityVersion { get; private set; }

    public bool Is64Bits { get; private set; }

    public string MonoLibraryName { get; private set; }

    public int AssemblyImage { get; private set; }

    public int ReferencedAssemblies { get; private set; }

    public int ImageClassCache { get; private set; }

    public int HashTableSize { get; private set; }

    public int HashTableTable { get; private set; }


    // MonoClass Offsets

    public int TypeDefinitionFieldSize { get; private set; }

    public int TypeDefinitionBitFields { get; private set; }

    public int TypeDefinitionClassKind { get; private set; }

    public int TypeDefinitionParent { get; private set; }

    public int TypeDefinitionNestedIn { get; private set; }

    public int TypeDefinitionName { get; private set; }

    public int TypeDefinitionNamespace { get; private set; }

    public int TypeDefinitionVTableSize { get; private set; }

    public int TypeDefinitionSize { get; private set; }

    public int TypeDefinitionFields { get; private set; }

    public int TypeDefinitionByValArg { get; private set; }

    public int TypeDefinitionRuntimeInfo { get; private set; }


    // MonoClassDef Offsets

    public int TypeDefinitionFieldCount { get; private set; }

    public int TypeDefinitionNextClassCache { get; private set; }


    // MonoClassGtd Offsets

    public int TypeDefinitionGenericContainer { get; private set; }


    // MonoClassRuntimeInfo Offsets

    public int TypeDefinitionRuntimeInfoDomainVTables { get; private set; }


    // MonoVTable Offsets

    public int VTable { get; private set; }


    public static MonoLibraryOffsets GetOffsets(string gameExecutableFilePath, bool force = true)
    {
      string unityVersion;
      if (gameExecutableFilePath.EndsWith(".exe"))
      {
        var peHeader = new PeNet.PeFile(gameExecutableFilePath);
        unityVersion = peHeader.Resources.VsVersionInfo.StringFileInfo.StringTable[0].FileVersion;

        // Taken from here https://stackoverflow.com/questions/1001404/check-if-unmanaged-dll-is-32-bit-or-64-bit;
        // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
        // Offset to PE header is always at 0x3C.
        // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
        // followed by a 2-byte machine type field (see the document above for the enum).
        //
        FileStream fs = new FileStream(gameExecutableFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);
        fs.Seek(0x3c, SeekOrigin.Begin);
        Int32 peOffset = br.ReadInt32();
        fs.Seek(peOffset, SeekOrigin.Begin);
        UInt32 peHead = br.ReadUInt32();

        if (peHead != 0x00004550) // "PE\0\0", little-endian
        {
          throw new Exception("Can't find PE header");
        }

        int machineType = br.ReadUInt16();
        br.Close();
        fs.Close();

        Console.WriteLine($"game file closed");

        switch (machineType)
        {
          case 0x8664: // IMAGE_FILE_MACHINE_AMD64
            return GetOffsets(unityVersion, true, force);
          case 0x14c: // IMAGE_FILE_MACHINE_I386
            return GetOffsets(unityVersion, false, force);
        }
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        gameExecutableFilePath = $"\"{gameExecutableFilePath}\"";
        Console.WriteLine($"DEBUG: MAIN MODULE = {gameExecutableFilePath}");

        FileInfo gameExecutableFile = new FileInfo(Path.GetRelativePath($"\"{Environment.CurrentDirectory}\"", gameExecutableFilePath));
        string infoPlist = File.ReadAllText(gameExecutableFile.Directory.Parent.FullName + "/Info.plist");
        string[] unityPlayerSplit = infoPlist.Split("Unity Player version ");
        unityVersion = unityPlayerSplit[1].Split(" ")[0];
        Console.WriteLine($"DEBUG: UNITY VERSION = {unityVersion}");

        // Start the child process.
        Process p = new Process();
        // Redirect the output stream of the child process.
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = "file";
        p.StartInfo.Arguments = gameExecutableFilePath;
        p.Start();
        // Do not wait for the child process to exit before
        // reading to the end of its redirected stream.
        // p.WaitForExit();
        // Read the output stream first and then wait.
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        Console.WriteLine($"DEBUG: file cmd output = {output}, contains 64-bit info = {output.Contains("Mach-O 64-bit executable x86_64\n")}");
        return GetOffsets(unityVersion, output.Contains("Mach-O 64-bit executable x86_64\n"), force);
      }
      throw new NotSupportedException("Platform not supported");
    }

    public static MonoLibraryOffsets GetOffsets(string unityVersion, bool is64Bits, bool force = true)
    {
      return GetOffsets(UnityVersion.Parse(unityVersion), is64Bits, force);
    }

    private static MonoLibraryOffsets GetOffsets(UnityVersion unityVersion, bool is64Bits, bool force = true)
    {
      MonoLibraryOffsets monoLibraryOffsets = SupportedVersions.Find(
             offsets => offsets.Is64Bits == is64Bits &&
                        offsets.UnityVersion == unityVersion
      );

      if (monoLibraryOffsets == null)
      {
        if (force)
        {
          List<MonoLibraryOffsets> matchingArchitectureSupportedVersion = SupportedVersions.FindAll(v => v.Is64Bits == is64Bits);
          if (matchingArchitectureSupportedVersion.Count == 1)
          {
            return matchingArchitectureSupportedVersion[0];
          }
          else if (matchingArchitectureSupportedVersion.Count > 1)
          {
            MonoLibraryOffsets bestCandidate = matchingArchitectureSupportedVersion[0];
            int bestCandidateYearDistance =
                Math.Abs(unityVersion.Year - bestCandidate.UnityVersion.Year);
            int bestCandidateVersionWithinYearDistance =
                Math.Abs(unityVersion.VersionWithinYear - bestCandidate.UnityVersion.VersionWithinYear);
            int bestCandidateSubversionWithinYearDistance =
                Math.Abs(unityVersion.SubversionWithinYear - bestCandidate.UnityVersion.SubversionWithinYear);
            for (int i = 1; i < matchingArchitectureSupportedVersion.Count; i++)
            {
              UnityVersion candidateVersion = matchingArchitectureSupportedVersion[i].UnityVersion;
              if (
                      Math.Abs(unityVersion.Year - candidateVersion.Year) < bestCandidateYearDistance
                  || (
                         Math.Abs(unityVersion.Year - candidateVersion.Year) == bestCandidateYearDistance
                      && (
                              Math.Abs(unityVersion.VersionWithinYear - candidateVersion.VersionWithinYear)
                                  < bestCandidateVersionWithinYearDistance
                          || (
                                  Math.Abs(unityVersion.VersionWithinYear - candidateVersion.VersionWithinYear)
                                      == bestCandidateVersionWithinYearDistance
                              && (
                                  Math.Abs(unityVersion.SubversionWithinYear - candidateVersion.SubversionWithinYear)
                                      < bestCandidateSubversionWithinYearDistance
                                 )
                             )
                         )
                     )
                  )
              {

                bestCandidate = matchingArchitectureSupportedVersion[i];
                bestCandidateYearDistance =
                    Math.Abs(unityVersion.Year - candidateVersion.Year);
                bestCandidateVersionWithinYearDistance =
                    Math.Abs(unityVersion.VersionWithinYear - candidateVersion.VersionWithinYear);
                bestCandidateSubversionWithinYearDistance =
                    Math.Abs(unityVersion.SubversionWithinYear - candidateVersion.SubversionWithinYear);
              }
            }
            return bestCandidate;
          }
        }

        string mode = is64Bits ? "in 64 bits mode" : "in 32 bits mode";
        throw new NotSupportedException($"The unity version the process is running " +
            $"({unityVersion} {mode}) is not supported");
      }

      Console.WriteLine($"Mono library selected = {monoLibraryOffsets.UnityVersion}");
      return monoLibraryOffsets;
    }
  }
}