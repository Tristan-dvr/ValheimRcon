using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class DeleteObjectById : RconCommand
    {
        public override string Command => "deleteObjectById";

        public override string Description => "Delete object by its user and object ids that make up an m_uid. Usage: findObjectById <userId> <objectId>";

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
            sb.AppendLine($" - Deleting object: {ZdoUtils.GetPrefabName(objects[0].GetPrefab())}");
            ZdoUtils.AppendZdoStats(objects[0], sb);
            ZdoUtils.deleteZDO(objects[0]);

            return sb.ToString().Trim();
        }
    }
}
