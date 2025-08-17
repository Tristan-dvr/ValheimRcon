using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class SpawnObject : RconCommand
    {
        public override string Command => "spawn";

        public override string Description =>"Spawns a prefab at the specified position with the given level and quality. " +
            "Usage: spawn <prefabName> <level> <count> <x> <y> <z>";

        protected override string OnHandle(CommandArgs args)
        {
            var prefabName = args.GetString(0);
            var level = args.GetInt(1);
            var count = args.GetInt(2);

            var position = new Vector3();
            position.x = args.GetInt(3);
            position.y = args.GetInt(4);
            position.z = args.GetInt(5);

            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (prefab == null) return $"Prefab {prefabName} not found";
            if (count <= 0) return "Nothing to spawn";

            for (int i = 0; i < count; i++)
            {
                ZNetView.StartGhostInit();

                var newPrefab = Object.Instantiate(prefab, position, Quaternion.identity);

                if (newPrefab.TryGetComponent<Character>(out var character))
                    character.SetLevel(level);
                if (newPrefab.TryGetComponent<ItemDrop>(out var itemDrop))
                    itemDrop.SetQuality(level);

                ZNetView.FinishGhostInit();
                Object.Destroy(newPrefab);
            }
            return $"{prefabName} x{count} level:{level} instantiated at {position}";
        }
    }
}
