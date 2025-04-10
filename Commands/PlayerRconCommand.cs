namespace ValheimRcon.Commands
{
    public abstract class PlayerRconCommand : RconCommand
    {
        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            var peer = ZNet.instance.GetPeerByHostName(user);
            if (peer == null) peer = ZNet.instance.GetPeerByPlayerName(user);

            if (peer == null) return $"Cannot find user {user}";

            var zdo = peer.GetZDO();
            if (zdo == null) return $"Cannot handle command for player {peer.GetPlayerInfo()}. ZDO not found";

            return OnHandle(peer, zdo, args);
        }

        protected abstract string OnHandle(ZNetPeer peer, ZDO zdo, CommandArgs args);
    }
}
