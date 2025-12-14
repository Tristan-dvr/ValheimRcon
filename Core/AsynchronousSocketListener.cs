using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ValheimRcon.Core
{
    internal sealed class AsynchronousSocketListener : IRconConnectionManager
    {
        private static readonly TimeSpan UnauthorizedClientLifetime = TimeSpan.FromSeconds(30);

        public event Action<IRconPeer, RconPacket> OnMessage;

        // Create a TCP/IP socket.  
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly Socket _listener;
        private readonly HashSet<IRconPeer> _clients = new HashSet<IRconPeer>();
        private readonly HashSet<IRconPeer> _waitingForDisconnect = new HashSet<IRconPeer>();
        private readonly SecurityReportHandler _securityReportHandler;
        private readonly IpAddressFilter _filter;
        private bool _checkNewConnections;

        public AsynchronousSocketListener(IPAddress ipAddress,
            int port,
            SecurityReportHandler securityReportHandler,
            IpAddressFilter filter)
        {
            if (ipAddress == null)
                throw new ArgumentNullException(nameof(ipAddress));

            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

            _address = ipAddress;
            _port = port;
            _filter = filter;

            _listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _securityReportHandler = securityReportHandler;
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

                _checkNewConnections = true;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Update()
        {
            TryAcceptNewClients();

            var now = DateTime.Now;
            foreach (var client in _clients)
            {
                if (!client.IsConnected())
                {
                    Disconnect(client);
                }
                else if (!client.Authentificated && now > client.Created + UnauthorizedClientLifetime)
                {
                    Log.Warning($"Unauthorized timeout [{client.Address}]");
                    _securityReportHandler?.Invoke(client.Address, Incident.UnexpectedBehaviour, "Unauthorized timeout.");
                    Disconnect(client);
                }
                else if (!_filter.IsAllowed(client.Address))
                {
                    Log.Warning($"Disconnected by IP filter [{client.Address}]");
                    _securityReportHandler?.Invoke(client.Address, Incident.IpFilter, "Disconnected by IP filter.");
                    Disconnect(client);
                }
                else if (client.TryReceive(out var packet, out var error))
                {
                    OnMessage?.Invoke(client, packet);
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    _securityReportHandler?.Invoke(client.Address, Incident.UnexpectedBehaviour, error);
                    Disconnect(client);
                }
            }

            // Process disconnections
            foreach (var client in _waitingForDisconnect)
            {
                _clients.Remove(client);
                DisconnectPeer(client);
            }
            _waitingForDisconnect.Clear();
        }

        public void Dispose()
        {
            _checkNewConnections = false;

            _listener.Close();

            foreach (var client in _clients)
                client.Dispose();
            _clients.Clear();

            _waitingForDisconnect.Clear();
        }

        public void Disconnect(IRconPeer peer)
        {
            _waitingForDisconnect.Add(peer);
        }

        private void TryAcceptNewClients()
        {
            if (!_checkNewConnections)
                return;

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
            var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            if (remoteEndPoint == null)
            {
                Log.Warning("Client connected with invalid endpoint");
                _securityReportHandler?.Invoke(socket.RemoteEndPoint, Incident.IpFilter, "Rejected connection. Unknown endpoint.");
                socket.Close();
                return;
            }

            var clientAddress = remoteEndPoint.Address;

            if (!_filter.IsAllowed(clientAddress))
            {
                Log.Warning($"Client connection rejected from [{remoteEndPoint}] - IP not allowed");
                _securityReportHandler?.Invoke(clientAddress, Incident.IpFilter, "Rejected connection by IP filter.");
                socket.Close();
                return;
            }

            var state = new RconPeer(socket);

            Log.Debug($"Client connected [{state.Address}]");
            _clients.Add(state);
        }

        private void DisconnectPeer(IRconPeer peer)
        {
            Log.Debug($"Client disconnected [{peer.Address}]");
            try
            {
                peer.Dispose();
            }
            catch
            {
                Log.Debug("Warning: Could not dispose peer connection");
            }
        }
    }
}