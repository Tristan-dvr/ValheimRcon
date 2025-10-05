using System.Linq;
using System.Text;
using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class FindObjectsNear : RconCommand
    {
        public override string Command => "findObjectsNear";

        public override string Description => "Find objects near a location. " +
            "Usage (with optional arguments): findObjectsNear <x> <y> <z> <radius> " +
            "-prefab <prefab> " +
            "-creator <creator id> " +
            "-id <id:userid> " +
            "-tag <tag>";

        protected override string OnHandle(CommandArgs args)
        {
            var position = args.GetVector3(0);
            var radius = args.GetFloat(3);

            long? creatorId = null;
            ObjectId? id = null;
            string tag = string.Empty;

            var optionalArgs = args.GetOptionalArguments();
            foreach (var index in optionalArgs)
            {
                var argument = args.GetString(index);
                switch (argument.ToLower())
                {
                    case "-creator":
                        creatorId = args.GetLong(index + 1);
                        break;
                    case "-id":
                        id = args.GetObjectId(index + 1);
                        break;
                    case "-tag":
                        tag = args.GetString(index + 1);
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }

            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo =>
                {
                    if (!IsInRange(zdo.GetPosition(), position, radius))
                    {
                        return false;
                    }

                    return ZdoUtils.MatchesCriteria(zdo, creatorId, id, tag);
                })
                .ToArray();

            if (objects.Length == 0)
            {
                return $"No objects found";
            }

            var sb = new StringBuilder();
            foreach (var zdo in objects)
            {
                sb.Append($"- Prefab: {ZdoUtils.GetPrefabName(zdo.GetPrefab())}");
                var distance = Vector3.Distance(position, zdo.GetPosition());
                sb.Append($" Distance: {distance}");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }

        private static bool IsInRange(Vector3 zdoPosition, Vector3 position, float radius)
        {
            return zdoPosition.x < position.x + radius
                && zdoPosition.x > position.x - radius
                && zdoPosition.y < position.y + radius
                && zdoPosition.y > position.y - radius
                && zdoPosition.z < position.z + radius
                && zdoPosition.z > position.z - radius;
        }
    }
}
