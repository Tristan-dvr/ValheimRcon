using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ValheimRcon.Core;

namespace ValheimRcon.Tests.Core
{
    [TestFixture]
    public class RconCommandReceiverTests
    {
        private MockRconConnectionManager _mockConnectionManager;
        private MockRconCommandHandler _mockCommandHandler;
        private RconCommandReceiver _receiver;
        private const string TestPassword = "testpassword123";

        [SetUp]
        public void SetUp()
        {
            Log.CreateInstance(new MockLogSource());
            _mockConnectionManager = new MockRconConnectionManager();
            _mockCommandHandler = new MockRconCommandHandler();
            _receiver = new RconCommandReceiver(_mockConnectionManager, TestPassword, new RconCommandHandler(_mockCommandHandler.HandleCommand), null);
        }

        [TearDown]
        public void TearDown()
        {
            _receiver?.Dispose();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithNullPassword_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new RconCommandReceiver(_mockConnectionManager, null, new RconCommandHandler(_mockCommandHandler.HandleCommand), null));
        }

        [Test]
        public void Constructor_WithEmptyPassword_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                new RconCommandReceiver(_mockConnectionManager, "", _mockCommandHandler.HandleCommand, null));
        }

        [Test]
        public void Constructor_WithNullCommandHandler_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new RconCommandReceiver(_mockConnectionManager, TestPassword, null, null));
        }

        #endregion

        #region Login Tests

        [Test]
        public void HandleMessage_LoginWithCorrectPassword_ShouldAuthenticate()
        {
            var peer = CreateMockPeer();
            var packet = new RconPacket(1, PacketType.Login, TestPassword);

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.IsTrue(peer.Authentificated);
            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual(1, response.requestId);
            Assert.AreEqual(PacketType.Command, response.type);
            Assert.AreEqual("Logic success", response.payload);
        }

        [TestCase("wrongpassword")]
        [TestCase(null)]
        [TestCase("      ")]
        [TestCase("")]
        public void HandleMessage_WrongPassword_ShouldReject(string password)
        {
            var peer = CreateMockPeer();
            var packet = new RconPacket(1, PacketType.Login, password);

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.IsFalse(peer.Authentificated);
            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual(-1, response.requestId);
            Assert.AreEqual("Login failed", response.payload);
        }

        [TestCase(TestPassword)]
        [TestCase("wrongpassword")]
        [TestCase(null)]
        [TestCase("      ")]
        [TestCase("")]
        public void HandleMessage_LoginWhenAlreadyAuthenticated_ShouldReject(string password)
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            var packet = new RconPacket(1, PacketType.Login, password);

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual(1, response.requestId);
            Assert.AreEqual("Already authorized", response.payload);
            Assert.IsTrue(_mockConnectionManager.DisconnectedPeers.Contains(peer));
        }

        #endregion

        #region Command Tests

        [TestCase("testcommand")]
        [TestCase("list")]
        [TestCase("save")]
        public void HandleMessage_CommandWithoutAuthentication_ShouldReject(string command)
        {
            var peer = CreateMockPeer();
            var packet = new RconPacket(1, PacketType.Command, command);

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual(1, response.requestId);
            Assert.AreEqual("Unauthorized", response.payload);
            Assert.IsTrue(_mockConnectionManager.DisconnectedPeers.Contains(peer));
        }

        [Test]
        public void HandleMessage_CommandWithAuthentication_ShouldProcess()
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            _mockCommandHandler.SetResponse("Command executed successfully");
            
            var packet = new RconPacket(1, PacketType.Command, "testcommand arg1 arg2");

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, _mockCommandHandler.HandledCommands.Count);
            var handledCommand = _mockCommandHandler.HandledCommands[0];
            Assert.AreEqual(peer, handledCommand.Peer);
            Assert.AreEqual("testcommand", handledCommand.Command);
            Assert.AreEqual(2, handledCommand.Arguments.Count);
            Assert.AreEqual("arg1", handledCommand.Arguments[0]);
            Assert.AreEqual("arg2", handledCommand.Arguments[1]);

            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual(1, response.requestId);
            Assert.AreEqual("Command executed successfully", response.payload);
        }

        [Test]
        public void HandleMessage_CommandWithSlashPrefix_ShouldStripSlash()
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            
            var packet = new RconPacket(1, PacketType.Command, "/testcommand");

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, _mockCommandHandler.HandledCommands.Count);
            var handledCommand = _mockCommandHandler.HandledCommands[0];
            Assert.AreEqual("testcommand", handledCommand.Command);
        }

        [Test]
        public void HandleMessage_EmptyCommand_ShouldReject()
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            
            var packet = new RconPacket(1, PacketType.Command, "");

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(0, _mockCommandHandler.HandledCommands.Count);
            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual("Empty command", response.payload);
        }

        [Test]
        public void HandleMessage_CommandWithQuotedArguments_ShouldParseCorrectly()
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            
            var packet = new RconPacket(1, PacketType.Command, "give player \"Sword Blackmetal\" 5");

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, _mockCommandHandler.HandledCommands.Count);
            var handledCommand = _mockCommandHandler.HandledCommands[0];
            Assert.AreEqual("give", handledCommand.Command);
            Assert.AreEqual(3, handledCommand.Arguments.Count);
            Assert.AreEqual("player", handledCommand.Arguments[0]);
            Assert.AreEqual("Sword Blackmetal", handledCommand.Arguments[1]);
            Assert.AreEqual("5", handledCommand.Arguments[2]);
        }

        [Test]
        public void HandleMessage_CommandWithMixedQuotedAndUnquotedArguments_ShouldParseCorrectly()
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            
            var packet = new RconPacket(1, PacketType.Command, "spawn Boar 100 50 200 \"-count\" 5 \"-tamed\"");

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, _mockCommandHandler.HandledCommands.Count);
            var handledCommand = _mockCommandHandler.HandledCommands[0];
            Assert.AreEqual("spawn", handledCommand.Command);
            Assert.AreEqual(7, handledCommand.Arguments.Count);
            Assert.AreEqual("Boar", handledCommand.Arguments[0]);
            Assert.AreEqual("100", handledCommand.Arguments[1]);
            Assert.AreEqual("50", handledCommand.Arguments[2]);
            Assert.AreEqual("200", handledCommand.Arguments[3]);
            Assert.AreEqual("-count", handledCommand.Arguments[4]);
            Assert.AreEqual("5", handledCommand.Arguments[5]);
            Assert.AreEqual("-tamed", handledCommand.Arguments[6]);
        }

        // ASCII characters
        [TestCase("A", 4051)]
        [TestCase("A", 5000)]
        [TestCase("A", 10000)]
        [TestCase("A", 50000)]
        [TestCase("A", 100000)]
        [TestCase("A", 500000)]
        [TestCase("A", 1000000)]
        [TestCase("A", 5000000)]
        [TestCase("A", 10000000)]
        // Multi-byte UTF-8 characters
        [TestCase("🎮", 1013)]
        [TestCase("🎮", 2000)]
        [TestCase("🎮", 3000)]
        [TestCase("🎮", 5000)]
        [TestCase("🎮", 10000)]
        [TestCase("🎮", 50000)]
        [TestCase("🎮", 100000)]
        [TestCase("🎮", 500000)]
        [TestCase("🎮", 1000000)]
        public void HandleMessage_CommandWithLongResponse_ShouldTruncate(string symbol, int length)
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            var longResponse = string.Concat(Enumerable.Repeat(symbol, length));
            _mockCommandHandler.SetResponse(longResponse);
            
            var packet = new RconPacket(1, PacketType.Command, "testcommand");

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.IsTrue(response.payload.Length < longResponse.Length);
            Assert.IsTrue(response.payload.EndsWith("--- message truncated ---"));
        }

        // ASCII characters
        [TestCase("A", 10)]
        [TestCase("A", 100)]
        [TestCase("A", 1000)]
        [TestCase("A", 4000)]
        [TestCase("A", 4050)]
        // Multi-byte UTF-8 characters
        [TestCase("🎮", 10)]
        [TestCase("🎮", 100)]
        [TestCase("🎮", 1000)]
        [TestCase("🎮", 1012)]
        public void HandleMessage_CommandWithValidResponse_ShouldSendFullResponse(string symbol, int length)
        {
            var peer = CreateMockPeer();
            peer.SetAuthentificated(true);
            var responseMessage = string.Concat(Enumerable.Repeat(symbol, length));
            _mockCommandHandler.SetResponse(responseMessage);
            
            var packet = new RconPacket(1, PacketType.Command, "testcommand");
            _mockConnectionManager.TriggerOnMessage(peer, packet);
            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual(responseMessage, response.payload);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void HandleMessage_UnknownPacketType_ShouldSendError()
        {
            var peer = CreateMockPeer();
            var packet = new RconPacket(1, (PacketType)999, "test");

            _mockConnectionManager.TriggerOnMessage(peer, packet);

            Assert.AreEqual(1, peer.SentPackets.Count);
            var response = peer.SentPackets[0];
            Assert.AreEqual(1, response.requestId);
            Assert.AreEqual(PacketType.Error, response.type);
            Assert.AreEqual("Cannot handle command", response.payload);
            Assert.IsTrue(_mockConnectionManager.DisconnectedPeers.Contains(peer));
        }

        #endregion

        #region Helper Methods

        private MockRconPeer CreateMockPeer()
        {
            return new MockRconPeer();
        }

        #endregion
    }

    #region Mock Classes

    public class MockRconConnectionManager : IRconConnectionManager
    {
        public List<RconPacket> SentPackets { get; } = new List<RconPacket>();
        public List<IRconPeer> DisconnectedPeers { get; } = new List<IRconPeer>();

        public event Action<IRconPeer, RconPacket> OnMessage;

        public void StartListening() { }

        public void Update() { }

        public void Disconnect(IRconPeer peer)
        {
            DisconnectedPeers.Add(peer);
        }

        public void Dispose() { }

        public void TriggerOnMessage(IRconPeer peer, RconPacket packet)
        {
            OnMessage?.Invoke(peer, packet);
        }
    }

    public class MockRconCommandHandler
    {
        public List<HandledCommand> HandledCommands { get; } = new List<HandledCommand>();
        private string _response = "Default response";

        public void SetResponse(string response)
        {
            _response = response;
        }

        public Task<string> HandleCommand(IRconPeer peer, string command, IReadOnlyList<string> data)
        {
            HandledCommands.Add(new HandledCommand
            {
                Peer = peer,
                Command = command,
                Arguments = data.ToList()
            });
            return Task.FromResult(_response);
        }
    }

    public class HandledCommand
    {
        public IRconPeer Peer { get; set; }
        public string Command { get; set; }
        public List<string> Arguments { get; set; }
    }

    public class MockRconPeer : IRconPeer
    {
        public bool Authentificated { get; private set; }
        public string Endpoint { get; } = "127.0.0.1:12345";
        public DateTime Created { get; } = DateTime.Now;
        public List<RconPacket> SentPackets { get; } = new List<RconPacket>();

        public void SetAuthentificated(bool authentificated)
        {
            Authentificated = authentificated;
        }

        public Task SendAsync(RconPacket packet)
        {
            SentPackets.Add(packet);
            return Task.CompletedTask;
        }

        public bool IsConnected()
        {
            return true;
        }

        public bool TryReceive(out RconPacket packet, out string error)
        {
            packet = default;
            error = default;
            return false;
        }

        public void Dispose()
        {
            // Mock implementation
        }
    }

    #endregion
}
