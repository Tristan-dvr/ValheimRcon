using System.Collections.Generic;
using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class ArmorStandZDOInfoProvider : IZDOInfoProvider
    {
        private readonly Dictionary<int, int> _itemStandSlots = new Dictionary<int, int>();
        private readonly ItemDrop.ItemData _tempData = new ItemDrop.ItemData();

        public void AppendInfo(ZDO zdo, StringBuilder stringBuilder)
        {
            stringBuilder.AppendFormat(" Pose: {0}", zdo.GetInt(ZDOVars.s_pose));
            stringBuilder.AppendFormat(" Attached items: ");

            var count = _itemStandSlots[zdo.GetPrefab()];
            var hasAny = false;
            for (var i = 0; i < count; i++)
            {
                var item = zdo.GetString($"{i}_item");
                if (string.IsNullOrEmpty(item))
                    continue;

                ItemDrop.LoadFromZDO(i, _tempData, zdo);

                stringBuilder.AppendFormat("( {0}", item);
                ZDOInfoUtil.AppendItemInfo(_tempData, stringBuilder);
                stringBuilder.Append(") ");

                hasAny = true;
            }

            if (!hasAny)
                stringBuilder.Append("<empty>");
        }

        public bool IsAvailableTo(ZDO zdo)
        {
            var prefabHash = zdo.GetPrefab();
            if (_itemStandSlots.TryGetValue(prefabHash, out var count))
                return count > 0;

            var prefab = ZNetScene.instance.GetPrefab(prefabHash);
            count = prefab != null && prefab.TryGetComponent<ArmorStand>(out var armorStand)
                ? armorStand.m_slots.Count
                : 0;
            _itemStandSlots[prefabHash] = count;
            return count > 0;
        }
    }
}