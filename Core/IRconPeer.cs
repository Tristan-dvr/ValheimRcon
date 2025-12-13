using System;
using System.Net;
using System.Threading.Tasks;

namespace ValheimRcon.Core
{
    public interface IRconPeer : IDisposable
    {
        bool Authentificated { get; }
        IPAddress Address { get; }
        DateTime Created { get; }
        void SetAuthentificated(bool authentificated);
        Task SendAsync(RconPacket packet);
        bool IsConnected();
        bool TryReceive(out RconPacket packet, out string error);
    }
}
