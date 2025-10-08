using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValheimRcon.Commands;

namespace ValheimRcon
{
    public static class ZdoUtils
    {
        [Flags]
        private enum Type
        {
            None = 0,
            ItemDrop = 1 << 0,
            GuardStone = 1 << 1,
            Character = 1 << 2,
            Building = 1 << 3,
            ItemStand = 1 << 4,
            Destructible = 1 << 5,
            Interactable = 1 << 6,
        };

        private static readonly Dictionary<int, Type> PrefabTypes = new Dictionary<int, Type>();
        private static readonly Dictionary<int, float> MaxHealth = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> MaxSupport = new Dictionary<int, float>();
        private static readonly int TagZdoHash = "valheim_rcon_object_tag".GetStableHashCode();

        public static string GetTag(this ZDO zdo) => zdo.GetString(TagZdoHash);

        public static void SetTag(this ZDO zdo, string tag) => zdo.Set(TagZdoHash, tag);

        public static void AppendZdoStats(ZDO zdo, StringBuilder stringBuilder)
        {
            stringBuilder.Append($" Id: {zdo.m_uid.ID}:{zdo.m_uid.UserID}");
            stringBuilder.Append($" Position: {zdo.GetPosition().ToDisplayFormat()}({ZoneSystem.GetZone(zdo.GetPosition())})");
            stringBuilder.Append($" Rotation: {zdo.GetRotation().ToDisplayFormat()}");

            var tag = zdo.GetTag();
            if (!string.IsNullOrEmpty(tag))
            {
                stringBuilder.Append($" Tag: {tag}");
            }

            var prefabId = zdo.GetPrefab();
            TryAppendItemDropData(zdo, stringBuilder);
            TryAppendBuildingData(zdo, stringBuilder);
            TryAppendCharacterData(zdo, stringBuilder);
            TryAppendGuardStoneData(zdo, stringBuilder);
            TryAppendItemStandData(zdo, stringBuilder);
        }

        public static string GetPrefabName(int prefabId)
        {
            var prefab = ZNetScene.instance.GetPrefab(prefabId);
            return prefab != null ? prefab.name : "Unknown";
        }

        public static string GetPrefabName(this ZDO zdo) => GetPrefabName(zdo.GetPrefab());

        public static void DeleteZDO(ZDO zdo)
        {
            if (!CanModifyZdo(zdo))
            {
                return;
            }
            zdo.SetOwner(ZDOMan.GetSessionID());

            var connectionId = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned);
            if (connectionId != ZDOID.None 
                && ZDOMan.instance.m_objectsByID.TryGetValue(connectionId, out var connectedZdo) 
                && connectedZdo != zdo)
            {
                DeleteZDO(connectedZdo);
            }

            ZDOMan.instance.DestroyZDO(zdo);
        }

        public static bool CanModifyZdo(ZDO zdo)
        {
            if (!zdo.IsValid())
            {
                return false;
            }
            if (ZNet.instance.m_peers.Any(p => p.m_characterID == zdo.m_uid)) // Player characters
            {
                return false;
            }
            var prefabName = GetPrefabName(zdo.GetPrefab());
            if (prefabName.StartsWith("_")) // System objects
            {
                return false;
            }
            if (GetPrefabTypes(zdo.GetPrefab()) == Type.None) // Unknown objects - be conservative
            {
                return false;
            }
            return true;
        }

        private static void TryAppendItemStandData(ZDO zdo, StringBuilder stringBuilder)
        {
            if (!CheckPrefabType(zdo.GetPrefab(), Type.ItemStand))
            {
                return;
            }
            string item = zdo.GetString(ZDOVars.s_item);
            if (string.IsNullOrEmpty(item))
            {
                return;
            }
            stringBuilder.Append($" Attached item: {item}");
            stringBuilder.Append($" Durability: {zdo.GetFloat(ZDOVars.s_durability)}");
            stringBuilder.Append($" Stack: {zdo.GetInt(ZDOVars.s_stack)}");
            stringBuilder.Append($" Quality: {zdo.GetInt(ZDOVars.s_quality)}");
            stringBuilder.Append($" Variant: {zdo.GetInt(ZDOVars.s_variant)}");
            stringBuilder.Append($" Crafter: {zdo.GetString(ZDOVars.s_crafterName)} ({zdo.GetLong(ZDOVars.s_crafterID)})");
            int dataCount = zdo.GetInt(ZDOVars.s_dataCount);
            if (dataCount > 0)
            {
                stringBuilder.Append($" Data:");
            }
            for (int i = 0; i < dataCount; i++)
            {
                stringBuilder.Append($" '{zdo.GetString($"data_{i}")}'='{zdo.GetString($"data__{i}")}'");
            }
        }

        private static void TryAppendItemDropData(ZDO zdo, StringBuilder stringBuilder)
        {
            if (!CheckPrefabType(zdo.GetPrefab(), Type.ItemDrop))
            {
                return;
            }
            stringBuilder.Append($" Durability: {zdo.GetFloat(ZDOVars.s_durability).ToDisplayFormat()}");
            stringBuilder.Append($" Stack: {zdo.GetInt(ZDOVars.s_stack)}");
            stringBuilder.Append($" Quality: {zdo.GetInt(ZDOVars.s_quality)}");
            stringBuilder.Append($" Variant: {zdo.GetInt(ZDOVars.s_variant)}");
            stringBuilder.Append($" Crafter: {zdo.GetString(ZDOVars.s_crafterName)} ({zdo.GetLong(ZDOVars.s_crafterID)})");
            stringBuilder.Append($" WorldLevel: {zdo.GetInt(ZDOVars.s_worldLevel)}");
            stringBuilder.Append($" PickedUp: {zdo.GetBool(ZDOVars.s_pickedUp)}");
            int dataCount = zdo.GetInt(ZDOVars.s_dataCount);
            if (dataCount > 0)
            {
                stringBuilder.Append($" Data:");
            }
            for (int i = 0; i < dataCount; i++)
            {
                stringBuilder.Append($" '{zdo.GetString($"data_{i}")}'='{zdo.GetString($"data__{i}")}'");
            }
        }

        private static void TryAppendGuardStoneData(ZDO zdo, StringBuilder stringBuilder)
        {
            if (!CheckPrefabType(zdo.GetPrefab(), Type.GuardStone))
            {
                return;
            }

            stringBuilder.Append($" Enabled: {zdo.GetBool(ZDOVars.s_enabled)}");
            stringBuilder.Append($" Owner: {zdo.GetString(ZDOVars.s_creatorName)}");
            stringBuilder.Append($" Permitted:");
            foreach (var player in GetPermittedPlayers(zdo))
            {
                stringBuilder.Append($" {player}");
            }
        }

        private static void TryAppendCharacterData(ZDO zdo, StringBuilder stringBuilder)
        {
            if (!CheckPrefabType(zdo.GetPrefab(), Type.Character))
            {
                return;
            }

            stringBuilder.Append($" Level: {zdo.GetInt(ZDOVars.s_level)}");
            var maxHealth = zdo.GetFloat(ZDOVars.s_maxHealth);
            stringBuilder.Append($" Health: {zdo.GetFloat(ZDOVars.s_health, maxHealth).ToDisplayFormat()}/{maxHealth.ToDisplayFormat()}");
            stringBuilder.Append($" Tamed: {zdo.GetBool(ZDOVars.s_tamed)}");
        }

        private static void TryAppendBuildingData(ZDO zdo, StringBuilder stringBuilder)
        {
            if (!CheckPrefabType(zdo.GetPrefab(), Type.Building))
            {
                return;
            }

            stringBuilder.Append($" Creator: {zdo.GetLong(ZDOVars.s_creator)}");
            var maxHealth = MaxHealth.TryGetValue(zdo.GetPrefab(), out var health) ? health : 0f;
            var maxSupport = MaxSupport.TryGetValue(zdo.GetPrefab(), out var support) ? support : 0f;
            stringBuilder.Append($" Health: {zdo.GetFloat(ZDOVars.s_health, maxHealth).ToDisplayFormat()}");
            stringBuilder.Append($" Support: {zdo.GetFloat(ZDOVars.s_support, maxSupport).ToDisplayFormat()}");
        }

        private static bool CheckPrefabType(int prefabId, Type type)
        {
            if (!ZNetScene.instance.HasPrefab(prefabId))
            {
                return false;
            }

            var types = GetPrefabTypes(prefabId);

            return (types & type) != 0;
        }

        private static Type GetPrefabTypes(int prefabId)
        {
            if (!ZNetScene.instance.HasPrefab(prefabId))
            {
                return Type.None;
            }

            if (!PrefabTypes.TryGetValue(prefabId, out var types))
            {
                var prefab = ZNetScene.instance.GetPrefab(prefabId);
                if (prefab.TryGetComponent<ItemDrop>(out _))
                {
                    types |= Type.ItemDrop;
                }
                if (prefab.TryGetComponent<Character>(out _))
                {
                    types |= Type.Character;
                }
                if (prefab.TryGetComponent<WearNTear>(out var wearNTear))
                {
                    types |= Type.Building;
                    MaxHealth[prefabId] = wearNTear.m_health;
                    MaxSupport[prefabId] = wearNTear.GetMaxSupport();
                }
                if (prefab.TryGetComponent<PrivateArea>(out _))
                {
                    types |= Type.GuardStone;
                }
                if (prefab.TryGetComponent<ItemStand>(out _))
                {
                    types |= Type.ItemStand;
                }
                if (prefab.TryGetComponent<IDestructible>(out _))
                {
                    types |= Type.Destructible;
                }
                if (prefab.TryGetComponent<Interactable>(out _))
                {
                    types |= Type.Interactable;
                }
                PrefabTypes[prefabId] = types;
            }

            return types;
        }

        private static IEnumerable<string> GetPermittedPlayers(ZDO zdo)
        {
            var count = zdo.GetInt(ZDOVars.s_permitted, 0);
            if (count <= 0)
            {
                yield break;
            }

            for (int i = 0; i < count; i++)
            {
                var id = zdo.GetLong($"pu_id{i}");
                var name = zdo.GetString($"pu_name{i}");
                yield return $"{id}({name})";
            }
        }
    }
}
