using System.Reflection;
using System.Text;
using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class SpawnObjectRotated : RconCommand
    {
        public override string Command => "spawnRotated";

        public override string Description =>"Spawns n prefabs at the specified position and rotation with the given level / quality. " +
            "Usage: spawn <prefabName> <level / quality (0-2 = normal, 3-5 = level +3 and no wear)> <count> <x> <y> <z> <rx> <rz> <ry> <tag> [<True for tamed / switchable>] [<name>]";

        protected override string OnHandle(CommandArgs args)
        {
            var result = new StringBuilder();
            var prefabName = args.GetString(0);
            var level = args.GetInt(1);
            bool trySetNoWearNTear = false;
            if(level > 3)
            {
                trySetNoWearNTear = true;
                level -= 3;
            }
            var count = args.GetInt(2);

            var position = new Vector3();
            position.x = args.GetFloat(3);
            position.y = args.GetFloat(4);
            position.z = args.GetFloat(5);

            var rotation = Quaternion.Euler(
                args.GetFloat(6),
                args.GetFloat(7),
                args.GetFloat(8)
            );
            string tag = args.GetString(9);
            bool tamed = false;
            if (args.Arguments.Count > 10 && bool.TryParse(args.Arguments[10], out var value)) {
                tamed = value;
            }
            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (prefab == null) {
                result.AppendLine($"Prefab {prefabName} not found");
                return result.ToString().Trim();
            }
            if (count <= 0)
            {
                result.AppendLine("Nothing to spawn");
                return result.ToString().Trim();
            }
            for (int i = 0; i < count; i++) {
                ZNetView.StartGhostInit();
                GameObject newPrefab = ZNetScene.Instantiate(prefab, position, rotation);
                if (newPrefab.TryGetComponent<Character>(out var character)) {
                    character.SetLevel(level);
                }
                if (newPrefab.TryGetComponent<ItemDrop>(out var itemDrop)) {
                    itemDrop.SetQuality(level);
                }
                if(trySetNoWearNTear && newPrefab.TryGetComponent<WearNTear>(out var wearNTearD))
                {
                    result.AppendLine("-WearNTear component is accessible directly, setting attributes");
                    wearNTearD.m_noSupportWear = true;
                    wearNTearD.m_noRoofWear = true;
                    wearNTearD.m_health = -1;
                }
                // TODO: Move this back out of ZNetView block below and test
                /*
                if (tamed && newPrefab.TryGetComponent<Tameable>(out var tameable)) {
                    tameable.m_character.m_tamed = true;
                    tameable.m_commandable = true;
                    if (args.Arguments.Count > 11) {
                        tameable.RPC_SetName(0, args.Arguments[11], "Odin");
                    }
                }
                */
                if (newPrefab.TryGetComponent<ZNetView>(out var znv) && znv.GetZDO() != null)
                {
                    znv.GetZDO().Set("tag", tag);
                    // TODO: m_ values are not reflected client side
                    if (trySetNoWearNTear && znv.TryGetComponent<WearNTear>(out var wearNTear))
                    {
                        result.AppendLine("-WearNTear component is accessible via ZNetView, setting attributes");
                        //wearNTear.RPC_HealthChanged(0, -1);
                        wearNTear.m_noSupportWear = true; // NOT WORKING
                        wearNTear.m_noRoofWear = true; // NOT WORKING
                        wearNTear.UpdateSupport(); // NOT WORKING
                        //wearNTear.m_health = -1f; // NOT WORKING
                        wearNTear.m_nview.GetZDO().Set(ZDOVars.s_health, 500000f);
                    }
                    if (tamed)
                    {
                        znv.GetZDO().Set(ZDOVars.s_tamed, true);
                        znv.GetZDO().Set(ZDOVars.s_health, -1);
                        //znv.GetZDO().Set(ZDOVars.s_follow, true); // TODO: Test
                        if (znv.TryGetComponent<Tameable>(out var tameable))
                        {
                            result.AppendLine("-Tameable is accessible, trying to set attributes");
                            tameable.m_character.m_tamed = true; // NOT WORKING
                            tameable.m_commandable = true; // NOT WORKING
                            // tameable.Tame(); // No difference when used with above
                            if (args.Arguments.Count > 11)
                            {
                                tameable.RPC_SetName(0, args.Arguments[11], "unknown");
                            }
                        }
                        if (znv.TryGetComponent<Fireplace>(out var fireplace))
                        {
                            // TODO: m_ values, etc are not reflected client side
                            result.AppendLine("-Fireplace is accessible, trying to set attributes");
                            //fireplace.SetFuel(0); // NOT WORKING
                            //fireplace.m_startFuel = 0; // NOT WORKING
                            //fireplace.SetFuel(0); // NOT WORKING
                            //fireplace.m_disableCoverCheck = true; // NOT WORKING
                            //fireplace.m_infiniteFuel = true; // NOT WORKING
                            //fireplace.m_canTurnOff = true; // NOT WORKING
                        }
                    }
                }
                ZNetView.FinishGhostInit();
                Object.Destroy(newPrefab);
            }
            result.AppendLine($"{prefabName} x{count} level:{level} instantiated at {position}");
            return result.ToString().Trim();
        }
    }
}
