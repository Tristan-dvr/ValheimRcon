using System;
using System.Net.Sockets;

namespace ValheimRcon.Core
{
    internal class RconPeer : IDisposable
    {
        internal const int BufferSize = 4096;
        private bool disposed = false;
        public bool IsDisposed => disposed;
        internal readonly byte[] Buffer = new byte[BufferSize];
        public readonly Socket socket;
        public readonly DateTime created;

        public bool Authentificated { get; private set; }
        public string Endpoint => socket.RemoteEndPoint?.ToString() ?? string.Empty;

        public RconPeer(Socket workSocket)
        {
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