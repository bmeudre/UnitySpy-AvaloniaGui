namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Net;  
    using System.Net.Sockets;  
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;

    /// <summary>
    /// A Windows specific facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacadeLinuxClient : ProcessFacadeLinux
    {
        public const int Port = 39185;
        public const int BufferSize = 4096;
        public const int RequestSize = 12;

        private byte[] internalBuffer = new byte[BufferSize];
        
        public ProcessFacadeLinuxClient(string mapsFilePath, string gameExecutableFilePath)
            : base(mapsFilePath, gameExecutableFilePath)
        {
            Console.WriteLine("Created Process Facade Linux Client");
        }

        protected override void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            int bufferIndex = 0;
            int length = size ?? buffer.Length;
            Request request = new Request(processAddress, length);
    
            // Connect to the server
            try
            {
                // Establish the remote endpoint for the socket.  
                IPHostEntry localhost = Dns.GetHostEntry(Dns.GetHostName());  
                IPAddress ipAddress = localhost.AddressList[0];  
                IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, ProcessFacadeLinuxClient.Port);  
    
                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
    
                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(serverEndPoint);  
    
                    // Send the data through the socket.  
                    int bytesSent = sender.Send(request.GetBytes());

                    // Receive the response from the remote device.  
                    int bytesRec;
                    do
                    {
                        bytesRec = sender.Receive(this.internalBuffer);
                        Array.Copy(this.internalBuffer, 0, buffer, bufferIndex, bytesRec);
                        bufferIndex += bytesRec;
                    }
                    while(bytesRec == BufferSize);

                    // Release the socket
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}",ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}",se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Request
        {
            private IntPtr address;
            private int size;

            public Request(IntPtr address, int size)
            {
                this.address = address;
                this.size = size;
            }

            public byte[] GetBytes() {
                byte[] arr = new byte[RequestSize];

                IntPtr ptr = Marshal.AllocHGlobal(RequestSize);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, arr, 0, RequestSize);
                Marshal.FreeHGlobal(ptr);
                return arr;
            }
        }
    }
}