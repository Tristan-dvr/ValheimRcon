using System;
using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class FindObjectsByTag : RconCommand
    {
        public override string Command => "findObjectsByTag";

        public override string Description => "Find objects by the their tag (i.e. Set(\"tag\") / GetString(\"tag\")). Usage: findObjectsByTag <tag>";

        protected override string OnHandle(CommandArgs args)
        {
            var tag = args.GetString(0);
            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => zdo.GetString("tag") == tag)
                .ToArray();

            if (objects.Length == 0) {
                return $"No objects found for tag {tag}";
            }

            var sb = new StringBuilder();
            foreach (var zdo in objects)
            {
                string prefabName = GetPrefabName(zdo.GetPrefab());
                sb.AppendLine($"- ID: ({zdo.m_uid.UserID} {zdo.m_uid.ID}), Tag: {zdo.GetString("tag")}, Prefab: {prefabName}");
                if (prefabName.StartsWith("itemstand", StringComparison.OrdinalIgnoreCase)) {
                    string item = zdo.GetString("item");
                    if (item == "") {
                        sb.AppendLine("  -> ItemStand is empty");
                    }
                    else {
                        int variant = zdo.GetInt("variant");
                        int quality = zdo.GetInt("quality");
                        string crafterName = zdo.GetString("crafterName");
                        sb.AppendLine($"  -> ItemStand contents: item = {item}, variant = {variant}, quality = {quality}, crafter = {crafterName}");
                    }
                }
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private static string GetPrefabName(int prefabId)
        {
            var prefab = ZNetScene.instance.GetPrefab(prefabId);
            return prefab != null ? prefab.name : "Unknown";
        }
    }
}
