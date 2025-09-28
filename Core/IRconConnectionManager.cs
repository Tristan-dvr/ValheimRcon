using System;

namespace ValheimRcon.Core
{
    public interface IRconConnectionManager : IDisposable
    {
        event Action<IRconPeer, RconPacket> OnMessage;

        void StartListening();
        void Update();
        void Disconnect(IRconPeer peer);
    }
}
