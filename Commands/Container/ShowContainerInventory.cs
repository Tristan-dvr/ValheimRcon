using System.Text;

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
                return $"Container {prefabName} (ID: {zdo.m_uid.ID}:{zdo.m_uid.UserID}) is empty.";
            }

            var sb = new StringBuilder();
            sb.AppendFormat("Prefab: {0}", prefabName);
            ZdoUtils.AppendZdoStats(zdo, sb);
            sb.AppendLine();
            sb.AppendFormat("Items ({0}):", inventory.NrOfItems());

            var allItems = inventory.GetAllItems();
            var itemIndex = 0;
            foreach (var item in allItems)
            {
                sb.AppendLine();

                var itemName = item.m_dropPrefab?.name ?? item.m_shared.m_name;
                sb.AppendFormat("[{0}] {1} x{2} quality:{3}", itemIndex, itemName, item.m_stack, item.m_quality);
                if (item.m_variant != 0)
                {
                    sb.AppendFormat(" variant:{0}", item.m_variant);
                }
                if (item.m_crafterID != 0)
                {
                    sb.AppendFormat(" crafter:{0}({1})", item.m_crafterName, item.m_crafterID);
                }
                if (item.m_customData.Count > 0)
                {
                    sb.AppendFormat(" data:");
                    var appendComma = false;
                    foreach (var pair in item.m_customData)
                    {
                        if (appendComma) sb.Append(',');
                        sb.AppendFormat("[{0}]:[{1}]", pair.Key, pair.Value);
                        appendComma = true;
                    }
                }
                itemIndex++;
            }

            return sb.ToString().TrimEnd();
        }
    }
}