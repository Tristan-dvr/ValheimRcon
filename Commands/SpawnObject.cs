using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class SpawnObject : RconCommand
    {
        public override string Command => "spawn";

        public override string Description => "Creates the specified number of objects at the given position. " +
            "Usage (with optional arguments): spawn <prefabName> <x> <y> <z> " +
            "-count(-c) <count> " +
            "-radius(-rad) <radius> " +
            "-level(-l) <level> " +
            "-rotation(-rot) <x> <y> <z> " +
            "-tag(-t) <tag> " +
            "-tamed ";

        protected override string OnHandle(CommandArgs args)
        {
            var prefabName = args.GetString(0);

            var position = args.GetVector3(1);

            int count = 1;
            int level = 0;
            string tag = string.Empty;
            Quaternion rotation = Quaternion.identity;
            float radius = 0f;
            bool tamed = false;

            foreach (var index in args.GetOptionalArguments())
            {
                var argument = args.GetString(index);
                switch (argument.ToLower())
                {
                    case "-level":
                    case "-l":
                        level = args.GetInt(index + 1);
                        break;
                    case "-count":
                    case "-c":
                        count = args.GetInt(index + 1);
                        break;
                    case "-tag":
                    case "-t":
                        tag = args.GetString(index + 1);
                        break;
                    case "-rotation":
                    case "-rot":
                        var eulerAngles = args.GetVector3(index + 1);
                        rotation = Quaternion.Euler(eulerAngles);
                        break;
                    case "-radius":
                    case "-rad":
                        radius = args.GetFloat(index + 1);
                        break;
                    case "-tamed":
                        tamed = true;
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }

            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (prefab == null) return $"Prefab {prefabName} not found";
            if (count <= 0) return "Nothing to spawn";

            var createdObjects = new List<ZDO>(count);
            for (int i = 0; i < count; i++)
            {
                ZNetView.StartGhostInit();

                var randomOffset = Random.onUnitSphere * radius;
                randomOffset.y = 0; // Keep on the horizontal plane
                var actualPosition = position + randomOffset;
                var newPrefab = Object.Instantiate(prefab, actualPosition, rotation);

                if (newPrefab.TryGetComponent<Character>(out var character))
                    character.SetLevel(level);
                if (newPrefab.TryGetComponent<ItemDrop>(out var itemDrop))
                    itemDrop.SetQuality(level);

                var zdo = newPrefab.GetComponent<ZNetView>().GetZDO();
                createdObjects.Add(zdo);
                if (!string.IsNullOrEmpty(tag))
                {
                    zdo.SetTag(tag);
                }
                if (tamed)
                {
                    zdo.Set(ZDOVars.s_tamed, true);
                }

                ZNetView.FinishGhostInit();
                Object.Destroy(newPrefab);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Spawned {count} objects:");
            foreach (var zdo in createdObjects)
            {
                sb.Append($"- Prefab: {ZdoUtils.GetPrefabName(zdo.GetPrefab())}");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }
    }
}
