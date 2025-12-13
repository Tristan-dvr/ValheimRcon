using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ValheimRcon.Core
{
    public class RconCommandReceiver : IDisposable
    {
        private static readonly Regex MatchRegex = new Regex(@"(?<=[ ][\""]|^[\""])[^\""]+(?=[\""][ ]|[\""]$)|(?<=[ ]|^)[^\"" ]+(?=[ ]|$)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IRconConnectionManager _manager;
        private readonly string _password;
        private readonly RconCommandHandler _commandHandler;
        private readonly SecurityReportHandler _securityReportHandler;

        public RconCommandReceiver(IRconConnectionManager connectionManager,
            string password,
            RconCommandHandler commandHandler,
            SecurityReportHandler securityReportHandler)
        {
            if (password == null)
                throw new ArgumentException("Password cannot be null", nameof(password));
            
            if (commandHandler == null)
                throw new ArgumentNullException(nameof(commandHandler));
            
            _password = password;
            _manager = connectionManager;
            _manager.OnMessage += SocketListener_OnMessage;
            _commandHandler = commandHandler;
            _securityReportHandler = securityReportHandler;
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
                            Log.Error($"Already authorized [{peer.Address}]");
                            _securityReportHandler?.Invoke(peer.Address, "Already authorized.");

                            await peer.SendAsync(new RconPacket(packet.requestId, PacketType.Command, "Already authorized"));
                            _manager.Disconnect(peer);
                            break;
                        }

                        RconPacket result;
                        var success = !string.IsNullOrWhiteSpace(_password)
                            && string.Equals(packet.payload ?? string.Empty, _password);
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
                        {
                            _securityReportHandler?.Invoke(peer.Address, "Login failed.");
                            _manager.Disconnect(peer);
                        }
                        break;
                    }
                case PacketType.Command:
                    {
                        if (!peer.Authentificated)
                        {
                            Log.Warning($"Not authorized [{peer.Address}]");
                            _securityReportHandler?.Invoke(peer.Address, "Unauthorized.");

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
                            Log.Warning($"Empty command from [{peer.Address}]");
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
                    Log.Error($"Unknown packet type: {packet} [{peer.Address}]");
                    _securityReportHandler?.Invoke(peer.Address, $"Unknown packet type {packet}.");

                    await peer.SendAsync(new RconPacket(packet.requestId, PacketType.Error, "Cannot handle command"));
                    _manager.Disconnect(peer);
                    break;
            }
        }
        
        private string ValidatePayloadLength(string payload)
        {
            const string truncatedMessage = "\n--- message truncated ---";

            var payloadSize = RconPacket.GetPayloadSize(payload);
            if (payloadSize > RconPacket.MaxPayloadSize)
            {
                var maxBytesForContent = RconPacket.MaxPayloadSize - RconPacket.GetPayloadSize(truncatedMessage);
                var truncatedPayload = RconCommandsUtil.TruncateMessageByBytes(payload, maxBytesForContent);
                return truncatedPayload + truncatedMessage;
            }

            return payload;
        }
    }
}
