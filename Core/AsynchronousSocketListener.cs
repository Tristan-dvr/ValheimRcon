using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ValheimRcon.Core
{
    internal class AsynchronousSocketListener
    {
        internal delegate void MessageReceived(RconPeer peer, RconPacket package);
        internal event MessageReceived OnMessage;

        // Create a TCP/IP socket.  
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly Socket _listener;
        private readonly List<RconPeer> _clients = new List<RconPeer>();
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

        public void Send(RconPeer peer, RconPacket packet)
        {
            var socket = peer.socket;
            var byteData = packet.Serialize();
            // Begin sending the data to the remote device.  
            socket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, socket);
        }

        public void Update()
        {
            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                var client = _clients[i];
                //  TODO:   maybe disconnect if there are no packages to receive long time
                if (client.socket.Connected && !IsDead(client.socket))
                {
                    TryReceive(client);
                }
                else
                {
                    Disconnect(client);
                    _clients.RemoveAt(i);
                }
            }
        }

        public void Close()
        {
            _listener.Close();
            _acceptThread?.Dispose();
            _clients.ForEach(c => c.Dispose());
            _clients.Clear();
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

            var ip = GetSocketEndPoint(socket);
            Log.Debug($"Client connected [{ip}]");

            //  Work with collection in main thread only
            ThreadingUtil.RunInMainThread(() => _clients.Add(state));
        }

        private void OnPackageReceived(RconPeer peer, int readCount)
        {
            var socket = peer.socket;
            var ip = GetSocketEndPoint(socket);
            Log.Debug($"Got package from client, {readCount} bytes [{ip}]");

            var package = new RconPacket(peer.Buffer);

            Log.Debug($"Received package {package}");
            OnMessage?.Invoke(peer, package);

            Array.Clear(peer.Buffer, 0, peer.Buffer.Length);
        }

        private void Disconnect(RconPeer peer)
        {
            var socket = peer.socket;
            var ip = GetSocketEndPoint(socket);
            Log.Debug($"Client disconnected [{ip}]");
            peer.Dispose();
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var socket = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = socket.EndSend(ar);
                var ip = GetSocketEndPoint(socket);
                Log.Debug($"Sent {bytesSent} bytes to client [{ip}]");

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private static bool IsDead(Socket socket)
        {
            return socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0;
        }

        private static string GetSocketEndPoint(Socket socket)
        {
            return socket.RemoteEndPoint?.ToString() ?? string.Empty;
        }
    }

}