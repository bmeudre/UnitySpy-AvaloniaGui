namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Net;  
    using System.Net.Sockets;  
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;
    using HackF5.UnitySpy.Util;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxClient : ProcessFacadeLinux
    {
        private readonly ProcessFacadeClient client;
        
        public ProcessFacadeLinuxClient(int processId, string gameExecutableFilePath)
            : base(processId, gameExecutableFilePath)
        {
            this.client = new ProcessFacadeClient();
        }

        protected override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
            => this.client.ReadProcessMemory(buffer, processAddress, length);
    }
}