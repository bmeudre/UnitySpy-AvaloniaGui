namespace HackF5.UnitySpy.Util
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Detail;

    internal static class Native
    {        
        // https://stackoverflow.com/questions/5497064/how-to-get-the-full-path-of-running-process
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        public static bool IsWow64Process(Process process)
        {
            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                if (!Environment.Is64BitOperatingSystem)
                    return false;
                // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead

                bool isWow64;
                if (!IsWow64Process(process.Handle, out isWow64))
                    throw new Exception("Something went wrong trying to figure out if the process is running a 64 bits or not");
                return !isWow64;
            }

            return false; // not on 64-bit Windows Emulator
        }
    }
}