using System.Text;

namespace ValheimRcon.Commands.Players
{
    internal class FindPlayer : RconCommand
    {
        public override string Command => "findPlayer";

        public override string Description => "Find a player and show their details. Usage: findPlayer <playername/steamid/player id>";

        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            var peer = ZNet.instance.GetPeerByPlayerName(user)
                ?? ZNet.instance.GetPeerByHostName(user);

            if (peer == null && long.TryParse(user, out var playerId))
            {
                peer = ZNet.instance.m_peers.Find(p => p.GetPlayerId() == playerId);
            }

            if (peer == null)
            {

                return $"Player {user} not found";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Player {user} found:");
            peer.WritePlayerInfo(sb);

            return sb.ToString().Trim();
        }
    }
}
