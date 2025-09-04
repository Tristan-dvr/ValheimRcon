using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class FindObjects : RconCommand
    {
        public override string Command => "findObjects";

        public override string Description => "Find objects matching all search criteria. " +
            "Usage (with optional arguments): findObjects " +
            "-prefab <prefab> " +
            "-creator <creator id> " +
            "-id <id> " +
            "-tag <tag>";

        protected override string OnHandle(CommandArgs args)
        {
            var optionalArgs = args.GetOptionalArguments();

            if (!optionalArgs.Any())
            {
                return "At least one search criteria must be provided.";
            }

            var prefab = string.Empty;
            long? creatorId = null;
            uint? id = null;
            var tag = string.Empty;

            foreach (var index in optionalArgs)
            {
                var argument = args.GetString(index);
                switch (argument.ToLower())
                {
                    case "-prefab":
                        prefab = args.GetString(index + 1);
                        break;
                    case "-creator":
                        creatorId = args.GetLong(index + 1);
                        break;
                    case "-id":
                        id = args.GetUInt(index + 1);
                        break;
                    case "-tag":
                        tag = args.GetString(index + 1);
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }
            var prefabHash = prefab.GetStableHashCode();

            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => ZdoUtils.MatchesCriteria(zdo, creatorId, id, tag))
                .ToArray();

            if (objects.Length == 0)
            {
                return "No objects found matching the provided criteria.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Found {objects.Length} objects:");
            foreach (var zdo in objects)
            {
                var prefabName = ZdoUtils.GetPrefabName(zdo.GetPrefab());
                sb.Append($"- Prefab: {prefabName}");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }
    }
}
