using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class FindObjectsCommand : RconCommand
    {
        public override string Command => "findObjects";

        public override string Description => "Finds all objects of a specific prefab in the world. Usage: findObjects <prefabName> [creatorId]. " +
            "If creatorId is specified, only objects created by that player will be returned.";

        protected override string OnHandle(CommandArgs args)
        {
            var objectName = args.GetString(0);
            var prefabId = objectName.GetStableHashCode();

            if (!ZNetScene.instance.HasPrefab(prefabId))
            {
                return $"Prefab with name '{objectName}' not found.";
            }

            var creatorId = args.TryGetInt(1, 0);
            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => zdo.GetPrefab() == prefabId);

            if (creatorId != 0)
            {
                objects = objects.Where(zdo => zdo.GetInt(ZDOVars.s_creator, 0) == creatorId);
            }

            var result = objects.ToArray();
            if (result.Length == 0)
            {
                return "No objects found";
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Found {result.Length} objects:");
            foreach (var zdo in result)
            {
                sb.Append($"- ID: ({zdo.m_uid.UserID} {zdo.m_uid.ID}), ");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }
    }
}
