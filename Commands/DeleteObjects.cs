using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValheimRcon.Commands.Search;

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

        private readonly List<ISearchCriteria> _criterias = new List<ISearchCriteria>();

        protected override string OnHandle(CommandArgs args)
        {
            var optionalArgs = args.GetOptionalArguments();
            var force = false;
            _criterias.Clear();
            foreach (var index in optionalArgs)
            {
                var argument = args.GetString(index);
                switch (argument.ToLower())
                {
                    case "-creator":
                        _criterias.Add(new CreatorCriteria(args.GetLong(index + 1)));
                        break;
                    case "-id":
                        _criterias.Add(new IdCriteria(args.GetObjectId(index + 1)));
                        break;
                    case "-tag":
                        _criterias.Add(new TagCriteria(args.GetString(index + 1)));
                        break;
                    case "-force":
                        force = true;
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }

            if (!_criterias.Any())
            {
                return "At least one criteria must be provided.";
            }

            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => _criterias.All(c => c.IsMatch(zdo)))
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

                if (!zdo.Persistent)
                {
                    sb.AppendLine(" [NOT ALLOWED TO DELETE]");
                }
                else if (force || ZdoUtils.CanModifyZdo(zdo))
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
