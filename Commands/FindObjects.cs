﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValheimRcon.Commands.Search;

namespace ValheimRcon.Commands
{
    internal class FindObjects : RconCommand
    {
        public override string Command => "findObjects";

        public override string Description => "Find objects matching all search criteria. " +
            "Usage (with optional arguments): findObjects " +
            "-near <x> <y> <z> <radius> " +
            "-prefab <prefab> " +
            "-creator <creator id> " +
            "-id <id:userid> " +
            "-tag <tag> " +
            "-tag-old <tag>";

        private readonly List<ISearchCriteria> _criterias = new List<ISearchCriteria>();

        protected override string OnHandle(CommandArgs args)
        {
            var optionalArgs = args.GetOptionalArguments();
            _criterias.Clear();
            foreach (var index in optionalArgs)
            {
                var argument = args.GetString(index);
                switch (argument.ToLower())
                {
                    case "-prefab":
                        _criterias.Add(new PrefabCriteria(args.GetString(index + 1)));
                        break;
                    case "-creator":
                        _criterias.Add(new CreatorCriteria(args.GetLong(index + 1)));
                        break;
                    case "-id":
                        _criterias.Add(new IdCriteria(args.GetObjectId(index + 1)));
                        break;
                    case "-tag":
                        _criterias.Add(new TagCriteria(args.GetString(index + 1)));
                        break;
                    case "-near":
                        _criterias.Add(new NearCriteria(args.GetVector3(index + 1), args.GetFloat(index + 4)));
                        break;
                    case "-tag-old":
                        _criterias.Add(new OldTagCriteria(args.GetString(index + 1)));
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
