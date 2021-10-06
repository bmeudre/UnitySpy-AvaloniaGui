namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeOSX : ProcessFacade
    {        
        protected readonly int processId;

        private readonly ProcessFacadeClient client;
        
        public ProcessFacadeOSX(int processId)
        {            
            this.processId = processId;
            this.client = new ProcessFacadeClient();
        }
                
        protected override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default) 
            => this.client.ReadProcessMemory(buffer, processAddress, size ?? buffer.Length);

        public override ModuleInfo GetMonoModule() 
        {            
            // TODO
            return null;
        }
    }
}