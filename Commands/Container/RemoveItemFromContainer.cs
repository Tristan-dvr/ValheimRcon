using System;

namespace ValheimRcon.Commands.Container
{
    internal class RemoveItemFromContainer : ContainerRconCommand
    {
        public override string Command => "removeItemFromContainer";

        public override string Description => "Removes an item from a container's inventory. " +
            "Usage: removeItemFromContainer <id:userid> " +
            "-index <index> " +
            "-item <name> " +
            "-count <count> " +
            "-force";

        protected override string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName)
        {
            var force = false;
            var count = 1;
            int? itemIndex = null;
            string itemName = null;
            
            foreach (var (index, argument) in args.GetOptionalArguments())
            {
                switch (argument)
                {
                    case "-index":
                        itemIndex = args.GetInt(index + 1);
                        if (itemIndex.Value < 0) return "Index must be at least 0";
                        break;
                    case "-item":
                        itemName = args.GetString(index + 1);
                        break;
                    case "-count":
                        count = args.GetInt(index + 1);
                        if (count < 1) return "Count must be at least 1";
                        break;
                    case "-force":
                        force = true;
                        break;
                    default:
                        return $"Unknown argument {argument}";
                }
            }

            if (!ContainerUtils.ValidateContainerModification(zdo, prefabName, force, out var validationError))
            {
                return validationError;
            }

            if (!itemIndex.HasValue && string.IsNullOrEmpty(itemName))
            {
                return "Either -index or -item must be specified.";
            }

            if (itemIndex.HasValue && !string.IsNullOrEmpty(itemName))
            {
                return "Cannot specify both -index and -item. Use only one.";
            }

            if (itemIndex.HasValue)
            {
                var allItems = inventory.GetAllItems();
                if (itemIndex.Value >= allItems.Count)
                {
                    return $"Index {itemIndex.Value} is out of range. Container has {allItems.Count} items (indices 0-{allItems.Count - 1}).";
                }

                var item = allItems[itemIndex.Value];
                var itemDropName = item.m_dropPrefab?.name ?? item.m_shared.m_name;
                var removedCount = Math.Min(count, item.m_stack);
                
                inventory.RemoveItem(item, removedCount);
                SaveInventory(zdo, inventory);
                return $"Removed {itemDropName} x{removedCount} from {prefabName} (index {itemIndex.Value})";
            }
            else
            {
                var itemDrop = ObjectDB.instance.GetItemPrefab(itemName);
                if (itemDrop == null)
                {
                    return $"Cannot find item prefab: {itemName}";
                }

                var itemDropName = itemDrop.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;

                if (!inventory.ContainsItemByName(itemDropName))
                {
                    return $"Item {itemName} not found in {prefabName}";
                }

                inventory.RemoveItem(itemDropName, count);
                SaveInventory(zdo, inventory);
                return $"Removed {itemDropName} x{count} from {prefabName}";
            }
        }
    }
}