using System.Text;

namespace ValheimRcon.Commands
{
    internal class FindPlayer : RconCommand
    {
        public override string Command => "findPlayer";

        public override string Description => "Find a player and show their details. Usage: findPlayer <playername or steamid>";

        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            var peer = ZNet.instance.GetPeerByPlayerName(user)
                ?? ZNet.instance.GetPeerByHostName(user);
            
            if (peer == null)
            {
                return $"Player {user} not found";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Player {user} found:");
            sb.AppendLine($"Steam ID: {peer.GetSteamId()}");
            sb.AppendLine($"Name: {peer.m_playerName}");
            var zdo = peer.GetZDO();
            var isDead = zdo == null;
            sb.AppendLine($"Is Dead: {isDead}");
            if (!isDead)
            {
                sb.AppendLine($"Position: {peer.GetRefPos()} ({ZoneSystem.GetZone(peer.GetRefPos())})");
                sb.AppendLine($"Health: {zdo.GetFloat(ZDOVars.s_health)}/{zdo.GetFloat(ZDOVars.s_maxHealth)}");
            }

            return sb.ToString().Trim();
        }
    }
}
