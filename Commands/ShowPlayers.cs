using System.Text;

namespace ValheimRcon.Commands
{
    internal class ShowPlayers : RconCommand
    {
        private StringBuilder _builder = new StringBuilder();

        public override string Command => "players";

        protected override string OnHandle(CommandArgs args)
        {
            _builder.Clear();
            var online = ZNet.instance.GetPeers().Count;
            _builder.AppendFormat("Online {0}\n", online);

            foreach (var player in ZNet.instance.GetPeers())
            {
                _builder.AppendFormat("{0}:{1} - {2}({3})",
                    player.GetSteamId(),
                    player.m_playerName,
                    player.GetRefPos(),
                    ZoneSystem.GetZone(player.GetRefPos()));

                _builder.AppendLine();
            }

            return _builder.ToString();
        }
    }
}
