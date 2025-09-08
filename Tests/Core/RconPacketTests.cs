using NUnit.Framework;
using ValheimRcon.Core;
using System;
using System.Text;
using System.IO;

namespace ValheimRcon.Tests.Core
{
    [TestFixture]
    public class RconPacketTests
    {
        #region Constructor Tests

        [TestCase(1, PacketType.Command, "test")]
        [TestCase(123, PacketType.Login, "password123")]
        [TestCase(0, PacketType.Error, "")]
        [TestCase(-1, PacketType.Command, "error message")]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly(int requestId, PacketType type, string payload)
        {
            var packet = new RconPacket(requestId, type, payload);

            Assert.AreEqual(requestId, packet.requestId);
            Assert.AreEqual(type, packet.type);
            Assert.AreEqual(payload, packet.payload);
        }

        [TestCase(null)]
        public void Constructor_WithNullPayload_ShouldInitializeCorrectly(string payload)
        {
            var packet = new RconPacket(1, PacketType.Command, payload);

            Assert.AreEqual(1, packet.requestId);
            Assert.AreEqual(PacketType.Command, packet.type);
            Assert.AreEqual(string.Empty, packet.payload);
        }

        #endregion

        #region Serialize Tests

        [TestCase(1, PacketType.Command, "test")]
        [TestCase(123, PacketType.Login, "password")]
        [TestCase(0, PacketType.Error, "")]
        [TestCase(-1, PacketType.Command, "error")]
        public void Serialize_WithValidPacket_ShouldReturnValidBytes(int requestId, PacketType type, string payload)
        {
            var packet = new RconPacket(requestId, type, payload);

            var bytes = packet.Serialize();

            Assert.IsNotNull(bytes);
            Assert.Greater(bytes.Length, 0);
            Assert.GreaterOrEqual(bytes.Length, 12); // Minimum packet size
        }

        [Test]
        public void Serialize_WithEmptyPayload_ShouldReturnValidBytes()
        {
            var packet = new RconPacket(1, PacketType.Command, "");

            var bytes = packet.Serialize();

            Assert.IsNotNull(bytes);
            Assert.AreEqual(14, bytes.Length); // Length(4) + RequestId(4) + Type(4) + Payload(0) + NullTerminator(2)
        }

        [Test]
        public void Serialize_WithLargePayload_ShouldReturnValidBytes()
        {
            var largePayload = new string('A', 1000);
            var packet = new RconPacket(1, PacketType.Command, largePayload);

            var bytes = packet.Serialize();

            Assert.IsNotNull(bytes);
            Assert.Greater(bytes.Length, 1000);
        }

        #endregion

        #region Deserialize Tests

        [TestCase(1, PacketType.Command, "test")]
        [TestCase(123, PacketType.Login, "password")]
        [TestCase(0, PacketType.Error, "")]
        [TestCase(-1, PacketType.Command, "error")]
        public void Constructor_FromBytes_ShouldParseCorrectly(int requestId, PacketType type, string payload)
        {
            var originalPacket = new RconPacket(requestId, type, payload);
            var bytes = originalPacket.Serialize();

            var parsedPacket = new RconPacket(bytes);

            Assert.AreEqual(originalPacket.requestId, parsedPacket.requestId);
            Assert.AreEqual(originalPacket.type, parsedPacket.type);
            Assert.AreEqual(originalPacket.payload, parsedPacket.payload);
        }

        [Test]
        public void Constructor_FromBytes_WithEmptyPayload_ShouldParseCorrectly()
        {
            var originalPacket = new RconPacket(1, PacketType.Command, "");
            var bytes = originalPacket.Serialize();

            var parsedPacket = new RconPacket(bytes);

            Assert.AreEqual(originalPacket.requestId, parsedPacket.requestId);
            Assert.AreEqual(originalPacket.type, parsedPacket.type);
            Assert.AreEqual(originalPacket.payload, parsedPacket.payload);
        }

        [Test]
        public void Constructor_FromBytes_WithUnicodePayload_ShouldParseCorrectly()
        {
            var unicodePayload = "тест 🎮 中文";
            var originalPacket = new RconPacket(1, PacketType.Command, unicodePayload);
            var bytes = originalPacket.Serialize();

            var parsedPacket = new RconPacket(bytes);

            Assert.AreEqual(originalPacket.requestId, parsedPacket.requestId);
            Assert.AreEqual(originalPacket.type, parsedPacket.type);
            Assert.AreEqual(originalPacket.payload, parsedPacket.payload);
        }

        #endregion

        #region Security Vulnerability Tests

        [Test]
        public void Constructor_FromBytes_WithNullBuffer_ShouldThrowException()
        {
            byte[] nullBytes = null;

            Assert.Throws<ArgumentNullException>(() => new RconPacket(nullBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithTooSmallBuffer_ShouldThrowException()
        {
            var smallBytes = new byte[] { 0x0C, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00 };

            Assert.Throws<ArgumentException>(() => new RconPacket(smallBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithTooLargeBuffer_ShouldThrowException()
        {
            var largeBytes = new byte[65537]; // 64KB + 1 byte
            largeBytes[0] = 0x0A; // length = 10
            largeBytes[1] = 0x00;
            largeBytes[2] = 0x00;
            largeBytes[3] = 0x00;

            Assert.Throws<ArgumentException>(() => new RconPacket(largeBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithTooSmallDataLength_ShouldThrowException()
        {
            var smallDataBytes = new byte[] { 0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00 };

            Assert.Throws<ArgumentException>(() => new RconPacket(smallDataBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithIntegerOverflowInLength_ShouldThrowException()
        {
            var overflowBytes = new byte[20];
            // Set length to int.MaxValue to test integer overflow protection
            overflowBytes[0] = 0xFF; // int.MaxValue = 0x7FFFFFFF, but we'll use 0xFFFFFFFF to test overflow
            overflowBytes[1] = 0xFF;
            overflowBytes[2] = 0xFF;
            overflowBytes[3] = 0xFF;

            Assert.Throws<ArgumentException>(() => new RconPacket(overflowBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithMaxValidLength_ShouldThrowException()
        {
            var maxLengthBytes = new byte[20];
            // Set length to int.MaxValue - 4 to test the boundary condition
            var maxLength = int.MaxValue - 4;
            maxLengthBytes[0] = (byte)(maxLength & 0xFF);
            maxLengthBytes[1] = (byte)((maxLength >> 8) & 0xFF);
            maxLengthBytes[2] = (byte)((maxLength >> 16) & 0xFF);
            maxLengthBytes[3] = (byte)((maxLength >> 24) & 0xFF);

            Assert.Throws<ArgumentException>(() => new RconPacket(maxLengthBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithZeroLength_ShouldThrowException()
        {
            // Arrange - Packet with zero length
            var zeroLengthBytes = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            Assert.Throws<ArgumentException>(() => new RconPacket(zeroLengthBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithExcessiveLength_ShouldThrowException()
        {
            // Arrange - Packet with excessive length that would cause buffer overflow
            var excessiveLengthBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };

            Assert.Throws<ArgumentException>(() => new RconPacket(excessiveLengthBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithInvalidUTF8_ShouldHandleGracefully()
        {
            var packet = new RconPacket(1, PacketType.Command, "test");
            var bytes = packet.Serialize();
            
            bytes[bytes.Length - 3] = 0xFF;
            bytes[bytes.Length - 2] = 0xFE;

            Assert.DoesNotThrow(() => new RconPacket(bytes));
        }

        [Test]
        public void Serialize_WithVeryLargePayload_ShouldThrowException()
        {
            // Arrange - Create packet with very large payload (potential memory exhaustion)
            var largePayload = new string('A', 1000000); // 1MB payload

            // Act & Assert - Should throw exception due to size limit when creating packet
            Assert.Throws<ArgumentException>(() => new RconPacket(1, PacketType.Command, largePayload));
        }

        [Test]
        public void Constructor_FromBytes_WithMalformedPacket_ShouldThrowException()
        {
            // Arrange - Malformed packet with incorrect structure
            var malformedBytes = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };

            Assert.Throws<ArgumentException>(() => new RconPacket(malformedBytes));
        }

        [Test]
        public void Constructor_FromBytes_WithNegativePayloadLength_ShouldThrowException()
        {
            // Arrange - Packet where payload length calculation results in negative value
            var negativePayloadBytes = new byte[] { 0x08, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };

            Assert.Throws<ArgumentException>(() => new RconPacket(negativePayloadBytes));
        }

        [Test]
        public void Constructor_WithTooLargePayload_ShouldThrowException()
        {
            var largePayload = new string('A', 5000);

            Assert.Throws<ArgumentException>(() => new RconPacket(1, PacketType.Command, largePayload));
        }

        [Test]
        public void Constructor_FromBytes_WithInvalidPacketType_ShouldThrowException()
        {
            var packet = new RconPacket(1, PacketType.Command, "test");
            var bytes = packet.Serialize();
            
            bytes[8] = 0xFF; // Corrupt packet type to invalid value
            bytes[9] = 0xFF;
            bytes[10] = 0xFF;
            bytes[11] = 0xFF;

            Assert.Throws<ArgumentException>(() => new RconPacket(bytes));
        }

        [Test]
        public void Serialize_WithNullPayload_ShouldWork()
        {
            var packet = new RconPacket(1, PacketType.Command, null);
            
            Assert.DoesNotThrow(() => packet.Serialize());
        }

        [Test]
        public void Serialize_WithTooLargePayload_ShouldThrowException()
        {
            var largePayload = new string('A', 5000);
            
            Assert.Throws<ArgumentException>(() => new RconPacket(1, PacketType.Command, largePayload));
        }

        #endregion

        #region GetPayloadSize Tests

        [TestCase("", 0)]
        [TestCase("test", 4)]
        [TestCase("hello world", 11)]
        [TestCase("тест", 8)] // UTF-8 encoding of Cyrillic characters
        [TestCase("🎮", 4)] // UTF-8 encoding of emoji
        public void GetPayloadSize_WithVariousStrings_ShouldReturnCorrectSize(string payload, int expectedSize)
        {
            var size = RconPacket.GetPayloadSize(payload);

            Assert.AreEqual(expectedSize, size);
        }

        [TestCase(null, 0)]
        public void GetPayloadSize_WithNullString_ShouldReturnZero(string payload, int expectedSize)
        {
            var size = RconPacket.GetPayloadSize(payload);

            Assert.AreEqual(expectedSize, size);
        }

        #endregion

        #region ToString Tests

        [TestCase(1, PacketType.Command, "test", "[1 t:Command test]")]
        [TestCase(123, PacketType.Login, "password", "[123 t:Login ****]")] // Password should be masked
        [TestCase(123, PacketType.Login, "password123123", "[123 t:Login ****]")] // Password should be masked
        [TestCase(0, PacketType.Error, "", "[0 t:Error ]")]
        [TestCase(-1, PacketType.Command, "error", "[-1 t:Command error]")]
        public void ToString_WithVariousPackets_ShouldReturnCorrectFormat(int requestId, PacketType type, string payload, string expected)
        {
            var packet = new RconPacket(requestId, type, payload);

            var result = packet.ToString();

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region Roundtrip Tests

        [TestCase(1, PacketType.Command, "test")]
        [TestCase(123, PacketType.Login, "password")]
        [TestCase(0, PacketType.Error, "")]
        [TestCase(-1, PacketType.Command, "error")]
        [TestCase(999, PacketType.Command, "very long payload with special characters: !@#$%^&*()")]
        public void Roundtrip_SerializeAndDeserialize_ShouldPreserveData(int requestId, PacketType type, string payload)
        {
            var originalPacket = new RconPacket(requestId, type, payload);

            var bytes = originalPacket.Serialize();
            var deserializedPacket = new RconPacket(bytes);

            Assert.AreEqual(originalPacket.requestId, deserializedPacket.requestId);
            Assert.AreEqual(originalPacket.type, deserializedPacket.type);
            Assert.AreEqual(originalPacket.payload, deserializedPacket.payload);
        }

        [Test]
        public void Roundtrip_WithSpecialCharacters_ShouldPreserveData()
        {
            var specialPayload = "Special chars: \n\r\t\"'\\\0";
            var originalPacket = new RconPacket(1, PacketType.Command, specialPayload);

            var bytes = originalPacket.Serialize();
            var deserializedPacket = new RconPacket(bytes);

            Assert.AreEqual(originalPacket.requestId, deserializedPacket.requestId);
            Assert.AreEqual(originalPacket.type, deserializedPacket.type);
            Assert.AreEqual(originalPacket.payload, deserializedPacket.payload);
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void Constructor_WithMaxIntRequestId_ShouldWork()
        {
            var maxInt = int.MaxValue;
            var packet = new RconPacket(maxInt, PacketType.Command, "test");

            var bytes = packet.Serialize();
            var deserializedPacket = new RconPacket(bytes);

            Assert.AreEqual(maxInt, deserializedPacket.requestId);
        }

        [Test]
        public void Constructor_WithMinIntRequestId_ShouldWork()
        {
            var minInt = int.MinValue;
            var packet = new RconPacket(minInt, PacketType.Command, "test");

            var bytes = packet.Serialize();
            var deserializedPacket = new RconPacket(bytes);

            Assert.AreEqual(minInt, deserializedPacket.requestId);
        }

        [Test]
        public void Constructor_WithAllPacketTypes_ShouldWork()
        {
            // Arrange & Act & Assert
            foreach (PacketType packetType in Enum.GetValues(typeof(PacketType)))
            {
                var packet = new RconPacket(1, packetType, "test");
                var bytes = packet.Serialize();
                var deserializedPacket = new RconPacket(bytes);
                
                Assert.AreEqual(packetType, deserializedPacket.type);
            }
        }

        #endregion
    }
}
