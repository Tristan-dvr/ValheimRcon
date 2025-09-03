using System;
using System.Collections.Generic;
using System.Text;

namespace ValheimRcon
{
    internal static class ZdoUtils
    {
        [Flags]
        private enum Type
        {
            None = 0,
            ItemDrop = 1,
            GuardStone = 2,
            Character = 4,
            Building = 8,
        };

        private static readonly Dictionary<int, Type> PrefabTypes = new Dictionary<int, Type>();
        private static readonly Dictionary<int, float> MaxHealth = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> MaxSupport = new Dictionary<int, float>();

        public static void AppendZdoStats(ZDO zdo, StringBuilder stringBuilder)
        {
            stringBuilder.Append($" Ids: {zdo.m_uid.UserID} {zdo.m_uid.ID}");
            stringBuilder.Append($" Position: {zdo.GetPosition()}({ZoneSystem.GetZone(zdo.GetPosition())})");
            stringBuilder.Append($" Rotation: {zdo.GetRotation().eulerAngles}");
            stringBuilder.Append($" Tag: {zdo.GetString("tag")}");
            var prefabId = zdo.GetPrefab();
            TryAppendItemDropData(zdo, stringBuilder);
            TryAppendBuildingData(zdo, stringBuilder);
            TryAppendCharacterData(zdo, stringBuilder);
            TryAppendGuardStoneData(zdo, stringBuilder);
        }

        public static string GetPrefabName(int prefabId)
        {
            var prefab = ZNetScene.instance.GetPrefab(prefabId);
            return prefab != null ? prefab.name : "Unknown";
        }

        public static void deleteZDO(ZDO obj)
        {
            obj.SetOwner(0);
            ZDOMan.instance.m_destroySendList.Add(obj.m_uid);
        }

        private static void TryAppendItemDropData(ZDO zdo, StringBuilder stringBuilder)
        {
            if (!CheckPrefabType(zdo.GetPrefab(), Type.ItemDrop))
            {
                return;
            }
            stringBuilder.Append($" Durability: {zdo.GetFloat(ZDOVars.s_durability)}");
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
            stringBuilder.Append($" Health: {zdo.GetFloat(ZDOVars.s_health, maxHealth)}/{maxHealth}");
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
            stringBuilder.Append($" Health: {zdo.GetFloat(ZDOVars.s_health, maxHealth)}");
            stringBuilder.Append($" Support: {zdo.GetFloat(ZDOVars.s_support, maxSupport)}");
        }

        private static bool CheckPrefabType(int prefabId, Type type)
        {
            if (!ZNetScene.instance.HasPrefab(prefabId))
            {
                return false;
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
                PrefabTypes[prefabId] = types;
            }

            return (types & type) != 0;
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
