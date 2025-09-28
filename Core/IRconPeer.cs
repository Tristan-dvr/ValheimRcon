using System;
using System.Threading.Tasks;

namespace ValheimRcon.Core
{
    public interface IRconPeer : IDisposable
    {
        bool Authentificated { get; }
        string Endpoint { get; }
        DateTime Created { get; }
        void SetAuthentificated(bool authentificated);
        Task SendAsync(RconPacket packet);
        bool IsConnected();
        bool TryReceive(out RconPacket packet);
    }
}
