using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class FindObjectById : RconCommand
    {
        public override string Command => "findObjectById";

        public override string Description => "Find an object by its user and object ids (which together make up a ZDO.m_uid). Usage: findObjectById <userId> <objectId>";

        protected override string OnHandle(CommandArgs args)
        {
            var userId = args.GetLong(0);
            var objectId = args.GetLong(1);
            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => zdo.m_uid.UserID == userId && zdo.m_uid.ID == objectId)
                .ToArray();

            if (objects.Length == 0)
            {
                return $"No objects found for m_uid {userId}:{objectId}";
            }

            var sb = new StringBuilder();
            sb.Append($"- Prefab: {ZdoUtils.GetPrefabName(objects[0].GetPrefab())}");
            ZdoUtils.AppendZdoStats(objects[0], sb);
            sb.AppendLine();

            return sb.ToString().Trim();
        }

    }
}
