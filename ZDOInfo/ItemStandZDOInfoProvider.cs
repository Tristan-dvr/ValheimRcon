using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class ItemStandZDOInfoProvider : ZDOInfoProviderBase<ItemStand>
    {
        private readonly ItemDrop.ItemData _tempData = new ItemDrop.ItemData();

        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            var itemHash = zdo.GetInt(ZDOVars.s_item, 0);
            stringBuilder.AppendFormat(" Attached item: ");
            if (itemHash == 0)
            {
                stringBuilder.Append("<empty>");
                return;
            }

            stringBuilder.Append(ZdoUtils.GetPrefabName(itemHash));
            if (detailed)
            {
                stringBuilder.Append(' ');
                ItemDrop.LoadFromZDO(_tempData, zdo);
                ZDOInfoUtil.AppendItemInfo(_tempData, stringBuilder);
            }
        }
    }
}