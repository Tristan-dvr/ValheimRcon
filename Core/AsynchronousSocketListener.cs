using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ValheimRcon.Core
{
    public sealed class AsynchronousSocketListener : IRconConnectionManager
    {
        private static readonly TimeSpan UnauthorizedClientLifetime = TimeSpan.FromSeconds(30);

        public event Action<IRconPeer, RconPacket> OnMessage;

        // Create a TCP/IP socket.  
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly Socket _listener;
        private readonly HashSet<IRconPeer> _clients = new HashSet<IRconPeer>();
        private readonly HashSet<IRconPeer> _waitingForDisconnect = new HashSet<IRconPeer>();
        private readonly List<IRconPeer> _clientsSnapshot = new List<IRconPeer>();
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
                if (!client.IsConnected())
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
                    if (client.TryReceive(out var packet))
                    {
                        OnMessage?.Invoke(client, packet);
                    }
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

        public void Dispose()
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

        public void Disconnect(IRconPeer peer)
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


        private void DisconnectPeer(IRconPeer peer)
        {
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

        private static bool IsUnauthorizedTimeout(IRconPeer peer)
        {
            return !peer.Authentificated
                && DateTime.Now - peer.Created > UnauthorizedClientLifetime;
        }
    }

}