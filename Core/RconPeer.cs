using System;
using System.Net.Sockets;

namespace ValheimRcon.Core
{
    public class RconPeer : IDisposable
    {
        public const int BufferSize = 4096;
        private bool disposed = false;
        public bool IsDisposed => disposed;
        public readonly byte[] Buffer = new byte[BufferSize];
        public readonly Socket socket;
        public readonly DateTime created;

        public bool Authentificated { get; private set; }
        public string Endpoint 
        { 
            get 
            { 
                try 
                { 
                    return socket?.RemoteEndPoint?.ToString() ?? string.Empty; 
                } 
                catch 
                { 
                    return "unknown"; 
                } 
            } 
        }

        public RconPeer(Socket workSocket)
        {
            if (workSocket == null)
                throw new ArgumentNullException(nameof(workSocket));
            
            socket = workSocket;
            created = DateTime.Now;
        }

        public void SetAuthentificated(bool authentificated) => Authentificated = authentificated;

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try { socket?.Shutdown(SocketShutdown.Both); } catch { }
            try { socket?.Close(); } catch { }
            socket.Dispose();
        }
    }
}