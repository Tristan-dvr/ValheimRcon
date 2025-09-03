using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class DeleteObjectsByTag : RconCommand
    {
        public override string Command => "deleteObjectsByTag";

        public override string Description => "Delete all objects with a matching tag value (run findAllObjectsByTag first). Usage: deleteAllObjectsByTag <tag>";

        protected override string OnHandle(CommandArgs args)
        {
            var tag = args.GetString(0);
            if (tag == null || tag == "") // Shouldn't happen due to the args checker but check just in case
            {
                return "Empty tag!";
            }
            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => zdo.GetString("tag") == tag)
                .ToArray();

            if (objects.Length == 0)
            {
                return $"No objects found for tag {tag}";
            }

            var sb = new StringBuilder();
            foreach (var zdo in objects)
            {
                sb.AppendLine($"Deleting object with m_uid {zdo.m_uid.UserID} {zdo.m_uid.ID}:");
                sb.Append($"- Prefab: {ZdoUtils.GetPrefabName(zdo.GetPrefab())}");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
                zdo.SetOwner(0);
                ZDOMan.instance.m_destroySendList.Add(zdo.m_uid);
            }

            return sb.ToString().Trim();
        }

    }
}
