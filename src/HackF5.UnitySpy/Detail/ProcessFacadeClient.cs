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
    public class ProcessFacadeClient
    {
        public const int Port = 39185;
        public const int BufferSize = 4096;
        public const int RequestSize = 12;

        private byte[] internalBuffer = new byte[BufferSize];

        private Socket socket;

        public void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            int length)
        {            
            int bufferIndex = 0;
            Request request = new Request(processAddress, length);
    
            try
            {    
                if(this.socket == null)
                {
                    Connect();
                }

                //Console.WriteLine($"Memory Chunk Requested: address = {request}");   

                // Send the data through the socket.  
                int bytesSent = this.socket.Send(request.GetBytes());

                // Receive the response from the remote device.  
                int bytesRec;
                do
                {
                    bytesRec = this.socket.Receive(this.internalBuffer);

                    Array.Copy(this.internalBuffer, 0, buffer, bufferIndex, bytesRec);
                    bufferIndex += bytesRec;
                    length -= bytesRec;
                }
                while(length > 0);          
                //Console.WriteLine("Memory Chunk Received");   
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}",ane.ToString());
                CloseConnection();
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}",se.ToString());
                CloseConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
                CloseConnection();
            }
        }

        // Connect to the server
        private void Connect() {
            Console.WriteLine("Connecting to server...");   

            // Establish the remote endpoint for the socket.  
            IPHostEntry localhost = Dns.GetHostEntry(Dns.GetHostName());  
            IPAddress ipAddress = localhost.AddressList[0];  
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, Port);  

            // Create a TCP/IP  socket.  
            this.socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
            this.socket.Connect(serverEndPoint);  
        }

        private void CloseConnection() 
        {
            if(socket != null) 
            {
                Console.WriteLine("Disconnecting from server...");   
                try
                {                    
                    // Release the socket
                    this.socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}",se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
                finally {
                    this.socket.Close();
                    this.socket = null;
                }
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

            public override string ToString()
            {
                return $"Address = {this.address.ToString("X")}, size = {this.size}";
            }
        }
    }
}