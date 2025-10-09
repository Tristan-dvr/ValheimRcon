namespace ValheimRcon.Commands.Admin
{
    internal class DisconnectAll : RconCommand
    {
        public override string Command => "disconnectAll";

        public override string Description => "Disconnects all players from the server.";

        protected override string OnHandle(CommandArgs args)
        {
            ZNet.instance.SendDisconnect();
            return "All players have been disconnected from the server.";
        }
    }
}
