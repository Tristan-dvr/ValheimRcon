using System.Linq;

namespace ValheimRcon
{
    public static class ZdoUtils
    {
        private static readonly int TagZdoHash = "valheim_rcon_object_tag".GetStableHashCode();

        public static string GetTag(this ZDO zdo) => zdo.GetString(TagZdoHash);

        public static void SetTag(this ZDO zdo, string tag) => zdo.Set(TagZdoHash, tag);

        public static void SetZdoModified(this ZDO zdo)
        {
            zdo.SetOwner(ZDOMan.GetSessionID());
            zdo.DataRevision += 100;
            ZDOMan.instance.ForceSendZDO(zdo.m_uid);
        }

        public static string GetPrefabName(int prefabId)
        {
            var prefab = ZNetScene.instance.GetPrefab(prefabId);
            return prefab != null ? prefab.name : $"Unknown ({prefabId})";
        }

        public static string GetPrefabName(this ZDO zdo) => GetPrefabName(zdo.GetPrefab());

        public static void DeleteZDO(ZDO zdo)
        {
            if (!zdo.Persistent)
            {
                return;
            }

            zdo.SetOwner(ZDOMan.GetSessionID());

            var connectionId = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned);
            if (connectionId != ZDOID.None
                && ZDOMan.instance.m_objectsByID.TryGetValue(connectionId, out var connectedZdo)
                && connectedZdo != zdo)
            {
                DeleteZDO(connectedZdo);
            }

            ZDOMan.instance.DestroyZDO(zdo);
        }

        public static bool CanModifyZdo(ZDO zdo)
        {
            if (!zdo.IsValid())
            {
                return false;
            }
            if (ZNet.instance.m_peers.Any(p => p.m_characterID == zdo.m_uid)) // Player characters
            {
                return false;
            }
            var prefabName = zdo.GetPrefabName();
            if (prefabName.StartsWith("_")) // System objects
            {
                return false;
            }
            return true;
        }
    }
}
