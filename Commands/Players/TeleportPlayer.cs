namespace ValheimRcon.Commands.Players
{
    internal class TeleportPlayer : PlayerRconCommand
    {
        public override string Command => "teleport";

        public override string Description => "Teleports the player to a specified position. Usage: teleport <x> <y> <z>";

        protected override string OnHandle(ZNetPeer peer, ZDO zdo, CommandArgs args)
        {
            var position = args.GetVector3(1);

            peer.InvokeRoutedRpcToZdo("RPC_TeleportTo", position, zdo.GetRotation(), true);
            return $"Player {peer.GetPlayerInfo()} teleported to {position.ToDisplayFormat()}";
        }
    }
}
