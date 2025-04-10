using System;
using System.Net.Sockets;

namespace ValheimRcon.Core
{
    internal class RconPeer : IDisposable
    {
        // Size of receive buffer.  
        internal const int BufferSize = 4096;

        // Receive buffer.  
        internal readonly byte[] Buffer = new byte[BufferSize];

        // Client socket.
        public readonly Socket socket;

        public bool Authentificated { get; private set; }

        public RconPeer(Socket workSocket)
        {
            socket = workSocket;
        }

        public void SetAuthentificated(bool authentificated) => Authentificated = authentificated;

        public void Dispose()
        {
            socket.Close();
            socket.Dispose();
        }
    }
}