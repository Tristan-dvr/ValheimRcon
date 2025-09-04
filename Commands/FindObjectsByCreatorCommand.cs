using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class FindObjectsByCreatorCommand : RconCommand
    {
        public override string Command => "findObjectsByCreator";

        public override string Description => "Find all objects created by a specific creator ID. Usage: findObjectsByCreator <creatorID>";

        protected override string OnHandle(CommandArgs args)
        {
            var creator = args.GetInt(0);

            if (creator == 0)
            {
                return "Invalid creator ID";
            }

            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => zdo.GetLong(ZDOVars.s_creator) == creator)
                .ToArray();

            if (objects.Length == 0)
            {
                return $"No objects found for creator {creator}";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Objects created by {creator}:");
            foreach (var zdo in objects)
            {
                sb.Append($"- Prefab: {ZdoUtils.GetPrefabName(zdo.GetPrefab())}");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

    }
}
