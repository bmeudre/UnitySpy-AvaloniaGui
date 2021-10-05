namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxPTrace : ProcessFacadeLinux
    {
        private readonly int processId;
        
        public ProcessFacadeLinuxPTrace(int processId, string mapsFilePath, string gameExecutableFilePath)
            : base(mapsFilePath, gameExecutableFilePath)
        {
            this.processId = processId;
        }

        protected unsafe override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
        {            
            fixed (byte* bytePtr = buffer)
            {
                var ptr = (IntPtr)bytePtr;
                var localIo = new iovec
                {
                    iov_base = ptr.ToPointer(),
                    iov_len = length
                };
                var remoteIo = new iovec
                {
                    iov_base = processAddress.ToPointer(),
                    iov_len = length
                };

                var res = process_vm_readv(processId, &localIo, 1, &remoteIo, 1, 0);
                if(res != -1)
                {
                    //Array.Copy(*(byte[]*)ptr, 0, buffer, 0, length);
                    Marshal.Copy(ptr, buffer, 0, length);
                }
                else
                {
                    throw new Exception("Error while trying to read memory through from process_vm_readv. Check errno.");
                }
            }
        }

        [DllImport("libc")]
        private static extern unsafe int process_vm_readv(int pid,
            iovec* local_iov,
            ulong liovcnt,
            iovec* remote_iov,
            ulong riovcnt,
            ulong flags);

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct iovec
        {
            public void* iov_base;
            public int iov_len;
        }

    }
}