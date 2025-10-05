using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class DeleteObjects : RconCommand
    {
        public override string Command => "deleteObjects";

        public override string Description => "Delete objects matching all search criteria. " +
            "Usage (with optional arguments): deleteObjects " +
            "-creator <creator id> " +
            "-id <id:userid> " +
            "-tag <tag> " +
            "-force (bypass security checks)";

        protected override string OnHandle(CommandArgs args)
        {
            var optionalArgs = args.GetOptionalArguments();

            if (!optionalArgs.Any())
            {
                return "At least one criteria must be provided.";
            }

            long? creatorId = null;
            ObjectId? id = null;
            var tag = string.Empty;
            var force = false;

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
                    case "-force":
                        force = true;
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }

            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => ZdoUtils.MatchesCriteria(zdo, creatorId, id, tag))
                .ToArray();

            if (objects.Length == 0)
            {
                return "No objects found matching the provided criteria.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Deleting {objects.Length} objects:");
            foreach (var zdo in objects)
            {
                var prefabName = ZdoUtils.GetPrefabName(zdo.GetPrefab());
                sb.Append($"- Prefab: {prefabName}");
                ZdoUtils.AppendZdoStats(zdo, sb);

                if (force || ZdoUtils.CanDeleteZdo(zdo))
                {
                    ZdoUtils.DeleteZDO(zdo);
                    sb.AppendLine(" [DELETED]");
                }
                else
                {
                    sb.AppendLine(" [NOT ALLOWED TO DELETE]");
                }

                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }
    }
}
