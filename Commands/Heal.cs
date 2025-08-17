namespace ValheimRcon.Commands
{
    internal class Heal : PlayerRconCommand
    {
        public override string Command => "heal";

        public override string Description => "Heals the player's to a specified value. Usage: heal <steamid> <value>";

        protected override string OnHandle(ZNetPeer peer, ZDO zdo, CommandArgs args)
        {
            var value = args.GetInt(1);
            peer.InvokeRoutedRpcToZdo("RPC_Heal", (float)value, true);
            return $"{peer.GetPlayerInfo()} healed to {value}hp";
        }
    }
}
