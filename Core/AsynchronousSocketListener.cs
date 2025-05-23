﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ValheimRcon.Core
{
    internal class AsynchronousSocketListener
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
        private IDisposable _acceptThread;

        public AsynchronousSocketListener(IPAddress ipAddress, int port)
        {
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
            try
            {
                var socket = peer.socket;
                var byteData = packet.Serialize();
                var bytesSent = await socket.SendAsync(new ArraySegment<byte>(byteData), SocketFlags.None);

                Log.Debug($"Sent {bytesSent} bytes to client [{peer.Endpoint}]");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Update()
        {
            foreach (var client in _clients)
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

            foreach (var client in _waitingForDisconnect)
            {
                _clients.Remove(client);
                DisconnectPeer(client);
            }
            _waitingForDisconnect.Clear();
        }

        public void Close()
        {
            _listener.Close();
            _acceptThread?.Dispose();
            foreach (var client in _clients)
                client.Dispose();
            _waitingForDisconnect.Clear();
            _clients.Clear();
        }

        public void Disconnect(RconPeer peer)
        {
            _waitingForDisconnect.Add(peer);
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

                OnPackageReceived(peer, readCount);
            }
        }

        private void OnClientConnected(Socket socket)
        {
            // Create the state object.  
            var state = new RconPeer(socket);

            Log.Debug($"Client connected [{state.Endpoint}]");

            //  Work with collection in main thread only
            ThreadingUtil.RunInMainThread(() => _clients.Add(state));
        }

        private void OnPackageReceived(RconPeer peer, int readCount)
        {
            var socket = peer.socket;
            Log.Debug($"Got package from client, {readCount} bytes [{peer.Endpoint}]");

            var package = new RconPacket(peer.Buffer);

            Log.Debug($"Received package {package}");
            OnMessage?.Invoke(peer, package);

            Array.Clear(peer.Buffer, 0, peer.Buffer.Length);
        }

        private void DisconnectPeer(RconPeer peer)
        {
            var socket = peer.socket;
            Log.Debug($"Client disconnected [{peer.Endpoint}]");
            peer.Dispose();
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