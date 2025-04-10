using System.IO;
using System.Text;

namespace ValheimRcon.Core
{
    internal readonly struct RconPacket
    {
        public readonly int requestId;
        public readonly PacketType type;
        public readonly string payload;

        public RconPacket(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                var length = reader.ReadInt32();
                requestId = reader.ReadInt32();
                type = (PacketType)reader.ReadInt32();
                var payloadBytes = reader.ReadBytes(length - sizeof(int) * 2 - 2);
                payload = Encoding.UTF8.GetString(payloadBytes);
            }
        }

        public RconPacket(int requestId, PacketType type, string payload)
        {
            this.requestId = requestId;
            this.type = type;
            this.payload = payload;
        }

        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(requestId);
                writer.Write((int)type);
                var payloadBytes = Encoding.UTF8.GetBytes(payload);
                writer.Write(payloadBytes);
                writer.Write((byte)0);
                writer.Write((byte)0);

                var data = stream.ToArray();
                stream.Position = 0;

                writer.Write(data.Length);
                writer.Write(data);
                return stream.ToArray();
            }
        }

        public override string ToString()
        {
            return $"[{requestId} t:{type} {payload}]";
        }
    }
}
