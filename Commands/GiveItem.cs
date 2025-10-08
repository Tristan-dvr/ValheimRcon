using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ValheimRcon.Commands
{
    internal class GiveItem : PlayerRconCommand
    {
        public override string Command => "give";

        public override string Description => "Spawns an item on the player. " +
            "Usage (with optional arguments): give <steamid> <item_name> " +
            "-count <count> " +
            "-quality <quality> " +
            "-variant <variant> " +
            "-data <key> <value> " +
            "-nocrafter";

        protected override string OnHandle(ZNetPeer peer, ZDO zdo, CommandArgs args)
        {
            var item = args.GetString(1);
            int count = 1;
            int quality = 1;
            int variant = 0;
            string crafterName = Plugin.ServerChatName.Value;
            long crafterId = -1;
            Dictionary<string, string> data = new Dictionary<string, string>();

            var prefab = ObjectDB.instance.GetItemPrefab(item);
            if (prefab == null) return $"Cannot find prefab {item}";

            var itemData = prefab.GetComponent<ItemDrop>().m_itemData;
            var sharedItemData = itemData.m_shared;

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
                        if (variant < 0)
                            return "Variant must be at least 0";
                        if (variant > 0 && sharedItemData.m_variants == 0)
                            return $"Item {item} does not have variants";
                        if (variant > sharedItemData.m_variants - 1)
                            return $"Item {item} has only {sharedItemData.m_variants} variants";
                        break;
                    case "-nocrafter":
                        crafterId = 0;
                        crafterName = string.Empty;
                        break;
                    case "-data":
                        {
                            var key = args.GetString(index + 1);
                            var value = args.TryGetString(index + 2);
                            data[key] = value;
                        }
                        break;
                    default:
                        return $"Unknown argument: {arg}";
                }
            }

            IDisposable disposer = ListPool<ItemDrop>.Get(out var spawnedItems);

            var totalCount = count;
            ZNetView.StartGhostInit();
            var sb = new StringBuilder();
            var position = zdo.GetPosition();
            sb.AppendLine($"Spawning items on player {peer.GetPlayerInfo()} {position.ToDisplayFormat()}:");
            while (count > 0)
            {
                var newItemData = itemData.Clone();
                var stackSize = Math.Min(sharedItemData.m_maxStackSize, count);
                newItemData.m_dropPrefab = prefab;
                newItemData.m_quality = quality;
                newItemData.m_variant = variant;
                newItemData.m_crafterID = crafterId;
                newItemData.m_crafterName = crafterName;
                newItemData.m_customData = data;
                if (sharedItemData.m_useDurability)
                    newItemData.m_durability = newItemData.GetMaxDurability();

                var dropped = ItemDrop.DropItem(newItemData, stackSize, position, Quaternion.identity);
                spawnedItems.Add(dropped);

                count -= stackSize;

                sb.Append('-');
                ZdoUtils.AppendZdoStats(dropped.m_nview.GetZDO(), sb);
                sb.AppendLine();
            }
            ZNetView.FinishGhostInit();

            foreach (var itemDrop in spawnedItems)
                Object.Destroy(itemDrop.gameObject);
            disposer.Dispose();

            return sb.ToString().TrimEnd();
        }
    }
}
