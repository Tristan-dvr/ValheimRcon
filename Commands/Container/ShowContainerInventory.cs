using System.Text;
using ValheimRcon.ZDOInfo;

namespace ValheimRcon.Commands.Container
{
    internal class ShowContainerInventory : ContainerRconCommand
    {
        public override string Command => "showContainer";

        public override string Description => "Shows inventory contents of a container by object ID. " +
            "Usage: showContainer <id:userid>";

        protected override string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName)
        {
            if (inventory.NrOfItems() == 0)
            {
                return "Container is empty.";
            }

            var sb = new StringBuilder();
            ZDOInfoUtil.AppendInfo(zdo, sb);
            sb.AppendLine();
            sb.AppendFormat("Items ({0}):", inventory.NrOfItems());

            var allItems = inventory.GetAllItems();
            var itemIndex = 0;
            foreach (var item in allItems)
            {
                sb.AppendLine();
                var itemName = item.m_dropPrefab?.name ?? item.m_shared.m_name;
                sb.AppendFormat("[{0}] {1} ", itemIndex, itemName);
                ZDOInfoUtil.AppendItemInfo(item, sb);
                itemIndex++;
            }

            return sb.ToString().TrimEnd();
        }
    }
}