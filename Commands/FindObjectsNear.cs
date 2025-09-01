using System.Linq;
using System.Text;
using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class FindObjectsNear : RconCommand
    {
        public override string Command => "findObjectsNear";

        public override string Description => "Find objects near a location. Usage: findObjectsNear <x> <z> <y> <r>";

        protected override string OnHandle(CommandArgs args)
        {
            var position = new Vector3();
            position.x = args.GetFloat(0);
            position.y = args.GetFloat(1);
            position.z = args.GetFloat(2);
            var radius = args.GetFloat(3);
            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo =>
                      zdo.GetPosition().x < position.x + radius
                   && zdo.GetPosition().x > position.x - radius
                   && zdo.GetPosition().y < position.y + radius
                   && zdo.GetPosition().y > position.y - radius
                   && zdo.GetPosition().z < position.z + radius
                   && zdo.GetPosition().z > position.z - radius
                )
                .ToArray();

            if (objects.Length == 0)
            {
                return $"No objects found";
            }

            var sb = new StringBuilder();
            foreach (var zdo in objects)
            {
                sb.Append($"- ID: ({zdo.m_uid.UserID} {zdo.m_uid.ID}), Tag: {zdo.GetString("tag")}, Prefab: {GetPrefabName(objects[0].GetPrefab())}");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }

        private static string GetPrefabName(int prefabId)
        {
            var prefab = ZNetScene.instance.GetPrefab(prefabId);
            return prefab != null ? prefab.name : "Unknown";
        }
    }
}
