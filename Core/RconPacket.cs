using System;
using System.IO;
using System.Text;

namespace ValheimRcon.Core
{
    public readonly struct RconPacket
    {
        public readonly int requestId;
        public readonly PacketType type;
        public readonly string payload;

        public RconPacket(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            if (bytes.Length < 14) // Minimum packet size: length(4) + requestId(4) + type(4) + null terminator(2)
                throw new ArgumentException("Packet too small", nameof(bytes));
            
            if (bytes.Length > 65536) // Limit buffer size to prevent DoS attacks
                throw new ArgumentException("Packet too large", nameof(bytes));

            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                var length = reader.ReadInt32();
                
                // Validate length
                if (length < 0)
                    throw new ArgumentException("Invalid packet length", nameof(bytes));
                
                // Minimum packet data length: requestId(4) + type(4) + null terminator(2) = 10 bytes
                if (length < 10)
                    throw new ArgumentException("Packet data too small", nameof(bytes));
                
                // Check if we have enough bytes for the declared length + 4 bytes for length field itself
                if (length > int.MaxValue - 4 || length + 4 > bytes.Length)
                    throw new ArgumentException("Packet length exceeds buffer size", nameof(bytes));
                
                requestId = reader.ReadInt32();
                type = (PacketType)reader.ReadInt32();
                
                // Validate packet type
                if (!Enum.IsDefined(typeof(PacketType), type))
                    throw new ArgumentException("Invalid packet type", nameof(bytes));
                
                // Calculate payload size safely
                const int headerSize = sizeof(int) * 2 + 2; // requestId + type + null terminator
                var payloadSize = length - headerSize;
                
                if (payloadSize < 0)
                    throw new ArgumentException("Invalid payload size", nameof(bytes));
                
                if (payloadSize > 4096) // Limit payload size to prevent memory exhaustion
                    throw new ArgumentException("Payload too large", nameof(bytes));
                
                var payloadBytes = reader.ReadBytes(payloadSize);
                payload = Encoding.UTF8.GetString(payloadBytes);
            }
        }

        public RconPacket(int requestId, PacketType type, string payload)
        {
            if (payload != null && GetPayloadSize(payload) > 4096)
                throw new ArgumentException("Payload too large", nameof(payload));
            
            this.requestId = requestId;
            this.type = type;
            this.payload = payload ?? string.Empty;
        }

        public byte[] Serialize()
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payload ?? string.Empty);
            
            if (payloadBytes.Length > 4096)
                throw new InvalidOperationException("Payload too large for serialization");
            
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Write length first (will be updated later)
                var lengthPosition = stream.Position;
                writer.Write(0); // Placeholder for length
                
                writer.Write(requestId);
                writer.Write((int)type);
                writer.Write(payloadBytes);
                writer.Write((byte)0);
                writer.Write((byte)0);

                // Calculate and write the actual length
                var totalLength = stream.Position - lengthPosition - 4; // Exclude length field itself
                if (totalLength > int.MaxValue)
                    throw new InvalidOperationException("Packet too large for serialization");
                
                var dataLength = (int)totalLength;
                stream.Position = lengthPosition;
                writer.Write(dataLength);
                
                return stream.ToArray();
            }
        }

        public static int GetPayloadSize(string payload)
        {
            if (payload == null)
                return 0;
            
            return Encoding.UTF8.GetByteCount(payload);
        }

        public override string ToString()
        {
            return $"[{requestId} t:{type} {payload}]";
        }
    }
}
