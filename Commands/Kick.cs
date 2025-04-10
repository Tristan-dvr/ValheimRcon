using System.Collections.Generic;

namespace ValheimRcon.Commands
{
    internal class Kick : RconCommand
    {
        public override string Command => "kick";

        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            ZNet.instance.Kick(user);
            return $"Kicked {user}";
        }
    }
}
