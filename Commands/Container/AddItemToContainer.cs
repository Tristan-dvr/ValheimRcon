using System.Collections.Generic;

namespace ValheimRcon.Commands.Container
{
    internal class AddItemToContainer : ContainerRconCommand
    {
        public override string Command => "addItemToContainer";

        public override string Description => "Adds an item to a container's inventory. " +
            "Usage: addItemToContainer <id:userid> <item_name> " +
            "-count <count> " +
            "-quality <quality> " +
            "-variant <variant> " +
            "-durability <durability> " +
            "-data <key> <value> " +
            "-nocrafter " +
            "-force";

        protected override string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName)
        {
            var itemName = args.GetString(1);
            var itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
            if (itemPrefab == null)
            {
                return $"Cannot find item prefab: {itemName}";
            }

            var itemData = itemPrefab.GetComponent<ItemDrop>().m_itemData;
            var sharedItemData = itemData.m_shared;

            var count = 1;
            var quality = 1;
            var variant = 0;
            var hasCrafter = true;
            var crafterName = Plugin.ServerChatName.Value;
            var crafterId = -1L;
            var customData = new Dictionary<string, string>();
            var force = false;
            float? durabilityOverride = null;

            var optionalArgs = args.GetOptionalArguments();
            foreach (var index in optionalArgs)
            {
                var arg = args.GetString(index);
                switch (arg)
                {
                    case "-count":
                        count = args.GetInt(index + 1);
                        if (count < 1) return "Count must be at least 1";
                        break;
                    case "-quality":
                        quality = args.GetInt(index + 1);
                        if (quality < 0) return "Quality must be at least 0";
                        break;
                    case "-variant":
                        variant = args.GetInt(index + 1);
                        if (variant < 0) return "Variant must be at least 0";
                        if (variant > 0 && sharedItemData.m_variants == 0)
                            return $"Item {itemName} does not have variants";
                        if (variant > sharedItemData.m_variants - 1)
                            return $"Item {itemName} has only {sharedItemData.m_variants} variants";
                        break;
                    case "-nocrafter":
                        hasCrafter = false;
                        crafterId = 0L;
                        crafterName = string.Empty;
                        break;
                    case "-data":
                        var key = args.GetString(index + 1);
                        var value = args.TryGetString(index + 2);
                        customData[key] = value;
                        break;
                    case "-durability":
                        durabilityOverride = args.GetFloat(index + 1);
                        if (durabilityOverride.Value < 0) return "Durability must be at least 0";
                        break;
                    case "-force":
                        force = true;
                        break;
                    default:
                        return $"Unknown argument: {arg}";
                }
            }

            if (!ContainerUtils.ValidateContainerModification(zdo, prefabName, force, out var validationError))
            {
                return validationError;
            }

            var clonedItem = itemData.Clone();
            clonedItem.m_quality = quality;
            clonedItem.m_variant = variant;
            clonedItem.m_stack = count;

            if (hasCrafter)
            {
                clonedItem.m_crafterID = crafterId;
                clonedItem.m_crafterName = crafterName;
            }
            else
            {
                clonedItem.m_crafterID = 0L;
                clonedItem.m_crafterName = string.Empty;
            }

            clonedItem.m_customData = customData;

            if (sharedItemData.m_useDurability)
            {
                clonedItem.m_durability = durabilityOverride.HasValue ? durabilityOverride.Value : clonedItem.GetMaxDurability();
            }

            if (inventory.AddItem(clonedItem))
            {
                SaveInventory(zdo, inventory);
                return $"Item {itemName} x{count} added to {prefabName}";
            }

            return $"Failed to add item to {prefabName}";
        }
    }
}