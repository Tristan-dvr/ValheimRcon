using System;

namespace ValheimRcon.Core
{
    [Flags]
    public enum Incident
    {
        IpFilter = 1 << 1,
        UnauthorizedAccess = 1 << 2,
        UnexpectedBehaviour = 1 << 3,
    }
}
