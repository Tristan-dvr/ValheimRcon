using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class ContainerZDOInfoProvider : ZDOInfoProviderBase<Container>
    {
        private static readonly Inventory TempInventory = new Inventory("", null, 8, 5);

        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            TempInventory.RemoveAll();
            var itemsData = zdo.GetString(ZDOVars.s_items);
            if (!string.IsNullOrEmpty(itemsData))
            {
                var package = new ZPackage(itemsData);
                TempInventory.Load(package);
            }
            var items = TempInventory.m_inventory;

            stringBuilder.Append("Container: ");
            var count = items.Count;
            if (count == 0)
            {
                stringBuilder.Append("<empty>");
            }
            else
            {
                stringBuilder.AppendFormat("{0} items", count);
            }
        }
    }
}