using System.Text;

namespace ValheimRcon.Commands.Players
{
    internal class ShowPlayers : RconCommand
    {
        private StringBuilder _builder = new StringBuilder();

        public override string Command => "players";

        public override string Description => "Show all online players with their positions and zones";

        protected override string OnHandle(CommandArgs args)
        {
            _builder.Clear();
            var online = ZNet.instance.GetPeers().Count;
            _builder.AppendFormat("Online {0}\n", online);

            foreach (var player in ZNet.instance.GetPeers())
            {
                player.WritePlayerInfo(_builder);

                _builder.AppendLine();
            }

            return _builder.ToString();
        }
    }
}
