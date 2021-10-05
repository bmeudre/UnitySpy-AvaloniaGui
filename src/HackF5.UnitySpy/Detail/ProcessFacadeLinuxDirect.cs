namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxDirect : ProcessFacadeLinux
    {
        private readonly string memFilePath;
        
        public ProcessFacadeLinuxDirect(string memFilePath, string mapsFilePath, string gameExecutableFilePath)
            : base(mapsFilePath, gameExecutableFilePath)
        {
            this.memFilePath = memFilePath;
        }

        protected unsafe override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
        {            
            using(FileStream memFileStream = new FileStream(memFilePath, FileMode.Create))
            {
                // Write the data to the file, byte by byte.
                for(int i = 0; i < length; i++)
                {
                    if(buffer[i] != memFileStream.ReadByte())
                    {
                        throw new Exception("Error reading data from "+memFilePath);
                    }
                }
            }
        }
    }
}