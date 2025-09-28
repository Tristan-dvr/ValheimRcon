using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ValheimRcon.Core
{
    public class RconCommandReceiver : IDisposable
    {
        private const int MaxPayloadSize = 4080;
        private static readonly Regex MatchRegex = new Regex(@"(?<=[ ][\""]|^[\""])[^\""]+(?=[\""][ ]|[\""]$)|(?<=[ ]|^)[^\"" ]+(?=[ ]|$)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IRconConnectionManager _manager;
        private readonly string _password;
        private readonly RconCommandHandler _commandHandler;

        public RconCommandReceiver(IRconConnectionManager connectionManager, string password, RconCommandHandler commandHandler)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
            if (commandHandler == null)
                throw new ArgumentNullException(nameof(commandHandler));
            
            _password = password;
            _manager = connectionManager;
            _manager.OnMessage += SocketListener_OnMessage;
            _commandHandler = commandHandler;
        }

        public void Update() => _manager.Update();

        public void Dispose()
        {
            _manager.OnMessage -= SocketListener_OnMessage;
        }

        private async void SocketListener_OnMessage(IRconPeer peer, RconPacket packet)
        {

            switch (packet.type)
            {
                case PacketType.Login:
                    {
                        if (peer.Authentificated)
                        {
                            Log.Error($"Already authorized [{peer.Endpoint}]");
                            await peer.SendAsync(new RconPacket(packet.requestId, PacketType.Command, "Already authorized"));
                            _manager.Disconnect(peer);
                            break;
                        }

                        RconPacket result;
                        var success = string.Equals(packet.payload?.Trim() ?? string.Empty, _password);
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
                        await peer.SendAsync(result);

                        if (!success)
                            _manager.Disconnect(peer);
                        break;
                    }
                case PacketType.Command:
                    {
                        if (!peer.Authentificated)
                        {
                            Log.Warning($"Not authorized [{peer.Endpoint}]");
                            await peer.SendAsync(new RconPacket(packet.requestId, packet.type, "Unauthorized"));
                            _manager.Disconnect(peer);
                            break;
                        }

                        // strip slash if present
                        var payload = packet.payload?.TrimStart('/') ?? string.Empty;

                        var data = MatchRegex.Matches(payload)
                            .Cast<Match>()
                            .Select(m => m.Value)
                            .ToList();
                        
                        if (data.Count == 0)
                        {
                            Log.Warning($"Empty command from [{peer.Endpoint}]");
                            await peer.SendAsync(new RconPacket(packet.requestId, packet.type, "Empty command"));
                            break;
                        }
                        
                        string command = data[0];
                        data.RemoveAt(0);

                        var response = await _commandHandler(peer, command, data);
                        response = ValidatePayloadLength(response);
                        var result = new RconPacket(packet.requestId, packet.type, response);
                        Log.Debug($"Command result {command} - {result}");
                        await peer.SendAsync(result);
                        break;
                    }
                default:
                    Log.Error($"Unknown packet type: {packet} [{peer.Endpoint}]");
                    await peer.SendAsync(new RconPacket(packet.requestId, PacketType.Error, "Cannot handle command"));
                    _manager.Disconnect(peer);
                    break;
            }
        }
        
        private string ValidatePayloadLength(string payload)
        {
            var payloadSize = RconPacket.GetPayloadSize(payload);
            if (payloadSize > MaxPayloadSize)
            {
                var newLength = MaxPayloadSize - 100;
                return RconCommandsUtil.TruncateMessage(payload, newLength)
                    + "\n--- message truncated ---";
            }

            return payload;
        }
    }
}
