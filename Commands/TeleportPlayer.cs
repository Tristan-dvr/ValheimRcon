using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class TeleportPlayer : PlayerRconCommand
    {
        public override string Command => "teleport";

        protected override string OnHandle(ZNetPeer peer, ZDO zdo, CommandArgs args)
        {
            var position = new Vector3();
            position.x = args.GetInt(1);
            position.y = args.GetInt(2);
            position.z = args.GetInt(3);

            peer.InvokeRoutedRpcToZdo("RPC_TeleportTo", position, Quaternion.identity, true);
            return $"Player {peer.GetPlayerInfo()} teleported to {position}";
        }
    }
}
