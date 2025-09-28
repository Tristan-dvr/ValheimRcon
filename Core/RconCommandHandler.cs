using System.Collections.Generic;
using System.Threading.Tasks;

namespace ValheimRcon.Core
{
    public delegate Task<string> RconCommandHandler(IRconPeer peer, string command, IReadOnlyList<string> data);
}
