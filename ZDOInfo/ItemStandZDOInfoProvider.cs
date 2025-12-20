using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class ItemStandZDOInfoProvider : ZDOInfoProviderBase<ItemStand>
    {
        private readonly ItemDrop.ItemData _tempData = new ItemDrop.ItemData();

        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder)
        {
            var item = zdo.GetString(ZDOVars.s_item);
            stringBuilder.AppendFormat(" Attached items:");
            if (string.IsNullOrEmpty(item))
            {
                stringBuilder.Append("<empty>");
                return;
            }

            stringBuilder.Append(item);
            ItemDrop.LoadFromZDO(_tempData, zdo);
            ZDOInfoUtil.AppendItemInfo(_tempData, stringBuilder);
        }
    }
}