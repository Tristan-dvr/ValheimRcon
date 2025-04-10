using UnityEngine;
using Object = UnityEngine.Object;

namespace ValheimRcon.Commands
{
    internal class GiveItem : PlayerRconCommand
    {
        public override string Command => "give";

        protected override string OnHandle(ZNetPeer peer, ZDO zdo, CommandArgs args)
        {
            var item = args.GetString(1);
            var quality = args.GetInt(2);
            var count = args.GetInt(3);

            var prefab = ObjectDB.instance.GetItemPrefab(item);
            if (prefab == null) return $"Cannot find prefab {item}";

            ZNetView.StartGhostInit();

            var itemData = prefab.GetComponent<ItemDrop>().m_itemData.Clone();
            itemData.m_dropPrefab = prefab;
            itemData.m_quality = quality;
            var dropped = ItemDrop.DropItem(itemData, count, peer.GetRefPos(), Quaternion.identity);

            ZNetView.FinishGhostInit();
            Object.Destroy(dropped.gameObject);

            return $"Item {item} x{count} spawned on player {peer.GetPlayerInfo()} {peer.GetRefPos()}";
        }
    }
}
