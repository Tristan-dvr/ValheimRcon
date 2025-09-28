using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ValheimRcon.Core
{
    public class RconPeer : IRconPeer
    {
        public const int BufferSize = 4096;

        private bool _disposed = false;
        private readonly byte[] _buffer = new byte[BufferSize];
        private readonly Socket _socket;

        public DateTime Created { get; }
        public bool Authentificated { get; private set; }
        public string Endpoint 
        { 
            get 
            { 
                if (_disposed)
                    return "unknown";
                    
                try 
                { 
                    return _socket?.RemoteEndPoint?.ToString() ?? string.Empty; 
                } 
                catch 
                { 
                    return "unknown"; 
                } 
            } 
        }

        public async Task SendAsync(RconPacket packet)
        {
            if (_disposed)
            {
                Log.Debug("Warning: Attempted to send to disposed peer");
                return;
            }

            if (_socket == null || !_socket.Connected)
            {
                Log.Debug("Warning: Socket is null or not connected");
                return;
            }

            var byteData = packet.Serialize();
            var bytesSent = await _socket.SendAsync(new ArraySegment<byte>(byteData), SocketFlags.None);
            Log.Debug($"Sent {bytesSent} bytes to client [{Endpoint}]");
        }

        public bool IsConnected()
        {
            if (_disposed)
                return false;

            return _socket.Connected
                && !(_socket.Poll(0, SelectMode.SelectRead) && _socket.Available == 0);
        }

        public bool TryReceive(out RconPacket packet)
        {
            packet = default;
            
            if (_disposed)
                return false;
            
            if (_socket.Poll(0, SelectMode.SelectRead) && _socket.Available > 0)
            {
                var availableBytes = _socket.Available;
                if (availableBytes > _buffer.Length)
                {
                    Log.Warning($"Available data exceeds buffer size: {availableBytes} > {_buffer.Length} [{Endpoint}]");
                    return false;
                }

                var readCount = _socket.Receive(_buffer, 0, Math.Min(availableBytes, _buffer.Length), SocketFlags.None);
                if (readCount == 0)
                    return false;

                try
                {
                    packet = new RconPacket(_buffer);
                    Log.Debug($"Received package {packet} from [{Endpoint}]");
                    return true;
                }
                catch (Exception e)
                {
                    Log.Warning($"Failed to parse packet from [{Endpoint}]: {e.Message}");
                    return false;
                }
                finally
                {
                    Array.Clear(_buffer, 0, _buffer.Length);
                }
            }
            return false;
        }

        public RconPeer(Socket workSocket)
        {
            if (workSocket == null)
                throw new ArgumentNullException(nameof(workSocket));
            
            _socket = workSocket;
            Created = DateTime.Now;
        }

        public void SetAuthentificated(bool authentificated) => Authentificated = authentificated;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _socket?.Shutdown(SocketShutdown.Both); } catch { }
            try { _socket?.Close(); } catch { }
            _socket.Dispose();
        }
    }
}