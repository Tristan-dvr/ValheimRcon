using System.Collections.Generic;
using System.Threading.Tasks;

namespace ValheimRcon.Core
{
    internal delegate Task<string> RconCommandHandler(string command, IReadOnlyList<string> data);
}
