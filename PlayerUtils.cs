namespace ValheimRcon
{
    public static class PlayerUtils
    {
        public static string GetPlayerInfo(this ZNetPeer peer)
        {
            return $"{peer.m_playerName}({peer.GetSteamId()})";
        }

        public static void InvokeRoutedRpcToZdo(this ZNetPeer peer, string rpc, params object[] args)
        {
            var zdo = peer.GetZDO();
            ZRoutedRpc.instance.InvokeRoutedRPC(zdo.GetOwner(), zdo.m_uid, rpc, args);
        }

        public static ZDO GetZDO(this ZNetPeer peer) => ZDOMan.instance.GetZDO(peer.m_characterID);

        public static string GetSteamId(this ZNetPeer peer) => peer.m_rpc.GetSocket().GetHostName();
    }
}
