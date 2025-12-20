using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValheimRcon.Commands.Search;
using ValheimRcon.ZDOInfo;

namespace ValheimRcon.Commands
{
    internal class DeleteObjects : RconCommand
    {
        public override string Command => "deleteObjects";

        public override string Description => "Delete objects matching all search criteria. " +
            "Usage (with optional arguments): deleteObjects " +
            "-near <x> <y> <z> <radius> " +
            "-prefab <prefab> " +
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
            var deletingByRadius = false;
            string deletingPrefab = null;
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
                    case "-prefab":
                        deletingPrefab = args.GetString(index + 1);
                        _criterias.Add(new PrefabCriteria(deletingPrefab));
                        break;
                    case "-near":
                        _criterias.Add(new NearCriteria(args.GetVector3(index + 1), args.GetFloat(index + 4)));
                        deletingByRadius = true;
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

            if (deletingByRadius && _criterias.Count == 1)
            {
                return "Must provide at least 1 more criteria if use -near";
            }

            if (!string.IsNullOrEmpty(deletingPrefab)
                && _criterias.Count == 1
                && ZNetScene.instance.GetPrefab(deletingPrefab) != null
                && !force)
            {
                return $"You're about to delete all existing objects of prefab {deletingPrefab} ({objects.Length}) in the world. Use -force if you really want to delete them.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Deleting {objects.Length} objects:");
            var deleted = 0;
            foreach (var zdo in objects)
            {
                sb.Append('-');
                ZDOInfoUtil.AppendInfo(zdo, sb);

                if (!zdo.Persistent)
                {
                    sb.AppendLine(" [NOT ALLOWED TO DELETE]");
                }
                else if (force || ZdoUtils.CanModifyZdo(zdo))
                {
                    ZdoUtils.DeleteZDO(zdo);
                    sb.AppendLine(" [DELETED]");
                    deleted++;
                }
                else
                {
                    sb.AppendLine(" [NOT ALLOWED TO DELETE]");
                }

                sb.AppendLine();
            }
            sb.AppendFormat("Deleted {0}/{1} objects", deleted, objects.Length);
            return sb.ToString().TrimEnd();
        }
    }
}
