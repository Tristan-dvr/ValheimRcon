using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class ItemDropZDOInfoProvider : ZDOInfoProviderBase<ItemDrop>
    {
        private readonly ItemDrop.ItemData _tempData = new ItemDrop.ItemData();

        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            ItemDrop.LoadFromZDO(_tempData, zdo);
            ZDOInfoUtil.AppendItemInfo(_tempData, stringBuilder);
        }
    }
}