using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ValheimRcon.Core
{
    internal class RconCommandReceiver : IDisposable
    {
        private static readonly Regex MatchRegex = new Regex(@"(?<=[ ][\""]|^[\""])[^\""]+(?=[\""][ ]|[\""]$)|(?<=[ ]|^)[^\"" ]+(?=[ ]|$)");

        private readonly AsynchronousSocketListener _socketListener;
        private readonly string _password;
        private RconCommandHandler _commandHandler;

        public RconCommandReceiver(int port, string password, RconCommandHandler commandHandler)
        {
            _password = password;
            _socketListener = new AsynchronousSocketListener(IPAddress.Any, port);
            _socketListener.OnMessage += SocketListener_OnMessage;
            _commandHandler = commandHandler;
        }

        public void StartListening() => _socketListener.StartListening();

        public void Update() => _socketListener.Update();

        public void Dispose() => _socketListener.Close();

        private async void SocketListener_OnMessage(RconPeer peer, RconPacket packet)
        {
            var socket = peer.socket;

            switch (packet.type)
            {
                case PacketType.Login:
                    {
                        RconPacket result;
                        if (string.Equals(packet.payload.Trim(), _password))
                        {
                            peer.SetAuthentificated(true);
                            result = new RconPacket(packet.requestId, PacketType.Command, "Logic success");
                        }
                        else
                        {
                            result = new RconPacket(-1, PacketType.Command, "Login failed");
                        }

                        //  TODO:   maybe disconnect right after sending packet if login failed
                        Log.Debug($"Login result {result}");
                        _socketListener.Send(peer, result);
                        break;
                    }
                case PacketType.Command:
                    {
                        // strip slash if present
                        var payload = packet.payload.TrimStart('/');

                        var data = MatchRegex.Matches(payload)
                            .Cast<Match>()
                            .Select(m => m.Value)
                            .ToList();
                        string command = data[0];
                        data.RemoveAt(0);

                        var response = peer.Authentificated
                            ? await _commandHandler(command, data)
                            : "Unauthorized";
                        //  TODO:   maybe disconnect right after sending packet if unauthorized

                        var result = new RconPacket(packet.requestId, packet.type, response);
                        Log.Debug($"Command result {command} - {result}");
                        _socketListener.Send(peer, result);
                        break;
                    }
                default:
                    //  TODO:   maybe disconnect right after sending packet
                    Log.Error($"Unknown packet type: {packet}");
                    _socketListener.Send(peer, new RconPacket(packet.requestId, PacketType.Error, "Cannot handle command"));
                    break;
            }
        }
    }
}
