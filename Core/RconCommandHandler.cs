using System.Collections.Generic;
using System.Threading.Tasks;

namespace ValheimRcon.Core
{
    public delegate Task<string> RconCommandHandler(RconPeer peer, string command, IReadOnlyList<string> data);
}
