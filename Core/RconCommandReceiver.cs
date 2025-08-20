using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ValheimRcon.Core
{
    internal class RconCommandReceiver : IDisposable
    {
        private const int MaxPayloadSize = 4080;
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
                        if (peer.Authentificated)
                        {
                            Log.Error($"Already authorized [{peer.Endpoint}]");
                            await _socketListener.SendAsync(peer, new RconPacket(packet.requestId, PacketType.Command, "Already authorized"));
                            _socketListener.Disconnect(peer);
                            break;
                        }

                        RconPacket result;
                        var success = string.Equals(packet.payload.Trim(), _password);
                        if (success)
                        {
                            peer.SetAuthentificated(true);
                            result = new RconPacket(packet.requestId, PacketType.Command, "Logic success");
                        }
                        else
                        {
                            result = new RconPacket(-1, PacketType.Command, "Login failed");
                        }

                        Log.Debug($"Login result {result}");
                        await _socketListener.SendAsync(peer, result);

                        if (!success)
                            _socketListener.Disconnect(peer);
                        break;
                    }
                case PacketType.Command:
                    {
                        if (!peer.Authentificated)
                        {
                            Log.Warning($"Not authorized [{peer.Endpoint}]");
                            await _socketListener.SendAsync(peer, new RconPacket(packet.requestId, packet.type, "Unauthorized"));
                            _socketListener.Disconnect(peer);
                            break;
                        }

                        // strip slash if present
                        var payload = packet.payload.TrimStart('/');

                        var data = MatchRegex.Matches(payload)
                            .Cast<Match>()
                            .Select(m => m.Value)
                            .ToList();
                        string command = data[0];
                        data.RemoveAt(0);

                        var response = await _commandHandler(peer, command, data);
                        var payloadSize = RconPacket.GetPayloadSize(response);
                        if (payloadSize > MaxPayloadSize)
                        {
                            var newLength = response.Length / 2;
                            response = RconCommandsUtil.TruncateMessage(response, newLength)
                                + "\n--- message truncated ---";
                        }

                        var result = new RconPacket(packet.requestId, packet.type, response);
                        Log.Debug($"Command result {command} - {result}");
                        await _socketListener.SendAsync(peer, result);
                        break;
                    }
                default:
                    Log.Error($"Unknown packet type: {packet} [{peer.Endpoint}]");
                    await _socketListener.SendAsync(peer, new RconPacket(packet.requestId, PacketType.Error, "Cannot handle command"));
                    _socketListener.Disconnect(peer);
                    break;
            }
        }
    }
}
