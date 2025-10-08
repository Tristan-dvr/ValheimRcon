﻿using System.Text;

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

        public static long GetPlayerId(this ZNetPeer peer) => peer.GetZDO()?.GetLong(ZDOVars.s_playerID) ?? 0L;

        public static void WritePlayerInfo(this ZNetPeer peer, StringBuilder sb)
        {
            sb.AppendFormat("{0} Steam ID:{1}", peer.m_playerName, peer.GetSteamId());
            sb.AppendFormat(" Position: {0}({1})", peer.GetRefPos().ToDisplayFormat(), ZoneSystem.GetZone(peer.GetRefPos()));
            var zdo = peer.GetZDO();
            if (zdo != null)
            {
                sb.AppendFormat(" Player ID:{0}", peer.GetPlayerId());
                sb.AppendFormat(" HP:{0}/{1}", zdo.GetFloat(ZDOVars.s_health).ToDisplayFormat(), zdo.GetFloat(ZDOVars.s_maxHealth).ToDisplayFormat());
            }
        }
    }
}
