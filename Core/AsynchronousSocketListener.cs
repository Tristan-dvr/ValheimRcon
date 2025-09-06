using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace ValheimRcon.Core
{
    public class AsynchronousSocketListener
    {
        private static readonly TimeSpan UnauthorizedClientLifetime = TimeSpan.FromSeconds(30);

        internal delegate void MessageReceived(RconPeer peer, RconPacket package);
        internal event MessageReceived OnMessage;

        // Create a TCP/IP socket.  
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly Socket _listener;
        private readonly HashSet<RconPeer> _clients = new HashSet<RconPeer>();
        private readonly HashSet<RconPeer> _waitingForDisconnect = new HashSet<RconPeer>();
        private readonly List<RconPeer> _clientsSnapshot = new List<RconPeer>();
        private readonly object _clientsLock = new object();
        private readonly object _disconnectLock = new object();
        private IDisposable _acceptThread;

        public AsynchronousSocketListener(IPAddress ipAddress, int port)
        {
            if (ipAddress == null)
                throw new ArgumentNullException(nameof(ipAddress));
            
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");
            
            _address = ipAddress;
            _port = port;
            _listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void StartListening()
        {
            Log.Message("Start listening rcon commands");
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                var localEndPoint = new IPEndPoint(_address, _port);
                _listener.Bind(localEndPoint);
                _listener.Listen(100);

                _acceptThread = ThreadingUtil.RunPeriodicalInSingleThread(TryAcceptClientThread, 100);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async Task SendAsync(RconPeer peer, RconPacket packet)
        {
            if (peer.IsDisposed)
            {
                Log.Debug("Tried to send to a disposed peer.");
                return;
            }

            string endpointString = "unknown";
            try
            {
                var socket = peer.socket;
                if (socket == null || !socket.Connected)
                {
                    Log.Debug("Warning: Socket is null or not connected");
                    return;
                }

                var byteData = packet.Serialize();
                var bytesSent = await socket.SendAsync(new ArraySegment<byte>(byteData), SocketFlags.None);
                endpointString = socket.RemoteEndPoint.ToString();
                Log.Debug($"Sent {bytesSent} bytes to client [{endpointString}]");
            }
            catch (ObjectDisposedException)
            {
                Log.Debug($"Attempted to send to a disposed socket ({endpointString}).");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Update()
        {
            // Create snapshot of clients for processing
            lock (_clientsLock)
            {
                _clientsSnapshot.Clear();
                _clientsSnapshot.AddRange(_clients);
            }

            // Process clients without holding the lock
            foreach (var client in _clientsSnapshot)
            {
                if (!IsConnected(client))
                {
                    Disconnect(client);
                }
                else if (IsUnauthorizedTimeout(client))
                {
                    Log.Warning($"Unauthorized timeout [{client.Endpoint}]");
                    Disconnect(client);
                }
                else
                {
                    TryReceive(client);
                }
            }

            // Process disconnections
            lock (_disconnectLock)
            {
                foreach (var client in _waitingForDisconnect)
                {
                    lock (_clientsLock)
                    {
                        _clients.Remove(client);
                    }
                    DisconnectPeer(client);
                }
                _waitingForDisconnect.Clear();
            }
        }

        public void Close()
        {
            _listener.Close();
            _acceptThread?.Dispose();
            
            lock (_clientsLock)
            {
                foreach (var client in _clients)
                    client.Dispose();
                _clients.Clear();
            }
            
            lock (_disconnectLock)
            {
                _waitingForDisconnect.Clear();
            }
        }

        public void Disconnect(RconPeer peer)
        {
            lock (_disconnectLock)
            {
                _waitingForDisconnect.Add(peer);
            }
        }

        private void TryAcceptClientThread()
        {
            try
            {
                if (!_listener.Poll(0, SelectMode.SelectRead))
                    return;

                var socket = _listener.Accept();
                OnClientConnected(socket);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void TryReceive(RconPeer peer)
        {
            var socket = peer.socket;
            if (socket.Poll(0, SelectMode.SelectRead) && socket.Available > 0)
            {
                var readCount = socket.Receive(peer.Buffer);

                if (readCount == 0)
                    return;

                // Validate read count to prevent buffer overflow
                if (readCount > peer.Buffer.Length)
                {
                    Log.Warning($"Received more data than buffer size: {readCount} > {peer.Buffer.Length} [{peer.Endpoint}]");
                    Disconnect(peer);
                    return;
                }

                OnPackageReceived(peer, readCount);
            }
        }

        private void OnClientConnected(Socket socket)
        {
            // Create the state object.  
            var state = new RconPeer(socket);

            Log.Debug($"Client connected [{state.Endpoint}]");

            //  Work with collection in main thread only
            ThreadingUtil.RunInMainThread(() => 
            {
                lock (_clientsLock)
                {
                    _clients.Add(state);
                }
            });
        }

        private void OnPackageReceived(RconPeer peer, int readCount)
        {
            var socket = peer.socket;
            Log.Debug($"Got package from client, {readCount} bytes [{peer.Endpoint}]");

            try
            {
                var package = new RconPacket(peer.Buffer);
                Log.Debug($"Received package {package}");
                OnMessage?.Invoke(peer, package);
            }
            catch (Exception e)
            {
                Log.Warning($"Failed to parse packet from [{peer.Endpoint}]: {e.Message}");
                Disconnect(peer);
            }
            finally
            {
                Array.Clear(peer.Buffer, 0, peer.Buffer.Length);
            }
        }

        private void DisconnectPeer(RconPeer peer)
        {
            var socket = peer.socket;
            Log.Debug($"Client disconnected [{peer.Endpoint}]");
            try
            {
                peer.Dispose();
            }
            catch
            {
                Log.Debug("Warning: Could not dispose peer connection");
            }
        }

        private static bool IsConnected(RconPeer peer)
        {
            var socket = peer.socket;
            return socket.Connected
                && !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
        }

        private static bool IsUnauthorizedTimeout(RconPeer peer)
        {
            return !peer.Authentificated
                && DateTime.Now - peer.created > UnauthorizedClientLifetime;
        }
    }

}