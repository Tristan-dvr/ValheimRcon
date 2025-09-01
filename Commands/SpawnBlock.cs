using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using static MeleeWeaponTrail;

namespace ValheimRcon.Commands
{
    public class Block
    {
        public string prefab { get; set; }
        public float x { get; set; }
        public float z { get; set; }
        public float y { get; set; }
        public float rx { get; set; }
        public float rz { get; set; }
        public float ry { get; set; }
    };

    internal class SpawnBlock : RconCommand
    {
        public override string Command => "spawnBlock";

        public override string Description =>"Spawns a \"building block\"" +
            "Usage: spawn <x> <y> <z> <rx> <ry> <rz> <tag>";

        protected override string OnHandle(CommandArgs args)
        {
            var result = new StringBuilder();
            var position = new Vector3();
            position.x = args.GetFloat(0);
            position.y = args.GetFloat(1);
            position.z = args.GetFloat(2);

            var rotation = Quaternion.Euler(
                args.GetFloat(3),
                args.GetFloat(4),
                args.GetFloat(5)
            );
            string tag = args.GetString(6);
            int style = 0;

            List<Block> block_parts = new List<Block> { };
            if (args.Arguments.Count > 7 && int.TryParse(args.Arguments[7], out var stylevalue))
            {
                style = stylevalue;
            }
            switch (style)
            {
                case 2:
                    block_parts.Add(new Block { prefab = "woodiron_beam_45" });
                    block_parts.Add(new Block { prefab = "woodiron_beam_45", rz = 90f });
                    block_parts.Add(new Block { prefab = "woodiron_beam_45", rz = 180f  });
                    block_parts.Add(new Block { prefab = "woodiron_beam_45", rz = 270f });
                    break;
                default:
                    block_parts.Add(new Block { prefab = "woodiron_pole" });
                    block_parts.Add(new Block { prefab = "woodiron_beam" });
                    block_parts.Add(new Block { prefab = "woodiron_beam", rz = 90f });
                    break;
            }
            int fill = 0;
            if (args.Arguments.Count > 8 && int.TryParse(args.Arguments[8], out var fillvalue))
            {
                fill = fillvalue;
            }
            switch(fill)
            {
                case 1:
                    block_parts.Add(new Block { prefab = "blackmarble_1x1" });
                    break;
                case 2:
                    block_parts.Add(new Block { prefab = "stone_wall_1x1" });
                    break;
                default:
                    break;
            }
            foreach (var block in block_parts)
            {
                var prefab = ZNetScene.instance.GetPrefab(block.prefab);
                if (prefab == null)
                {
                    result.AppendLine($"Prefab {block.prefab} not found");
                    return result.ToString().Trim();
                }
                Vector3 offset = position;
                offset.x += block.x;
                offset.z += block.z;
                offset.y += block.y;
                Quaternion rotate = Quaternion.Euler(block.rx, block.rz, block.ry);
                Quaternion rotated = rotation * rotate;
                ZNetView.StartGhostInit();
                GameObject newPrefab = ZNetScene.Instantiate(prefab, offset, rotated);
                bool trySetNoWearNTear = true;
                if(trySetNoWearNTear && newPrefab.TryGetComponent<WearNTear>(out var wearNTearD))
                {
                    wearNTearD.m_noSupportWear = true;
                    wearNTearD.m_noRoofWear = true;
                    wearNTearD.m_health = -1;
                }
                if (newPrefab.TryGetComponent<ZNetView>(out var znv) && znv.GetZDO() != null)
                {
                    znv.GetZDO().Set("tag", tag);
                    // TODO: m_ values are not reflected client side
                    if (trySetNoWearNTear && znv.TryGetComponent<WearNTear>(out var wearNTear))
                    {
                        //wearNTear.RPC_HealthChanged(0, -1);
                        wearNTear.m_noSupportWear = true; // NOT WORKING
                        wearNTear.m_noRoofWear = true; // NOT WORKING
                        wearNTear.UpdateSupport(); // NOT WORKING
                        //wearNTear.m_health = -1f;  // NOT WORKING
                        wearNTear.m_nview.GetZDO().Set(ZDOVars.s_health, 500000f);
                    }

                }
                UnityEngine.Object.Destroy(newPrefab);
                ZNetView.FinishGhostInit();
            }
            result.AppendLine($"Block instantiated at {position}");
            return result.ToString().Trim();

        }
    }
}
