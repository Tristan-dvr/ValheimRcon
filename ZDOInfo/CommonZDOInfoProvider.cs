using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class CommonZDOInfoProvider : IZDOInfoProvider
    {
        public void AppendInfo(ZDO zdo, StringBuilder stringBuilder)
        {
            stringBuilder.AppendFormat(" Prefab: {0}", zdo.GetPrefabName());
            stringBuilder.AppendFormat(" Id: {0}:{1}", zdo.m_uid.ID, zdo.m_uid.UserID);
            stringBuilder.AppendFormat(" Position: {0}({1})", zdo.GetPosition().ToDisplayFormat(), ZoneSystem.GetZone(zdo.GetPosition()));
            stringBuilder.AppendFormat(" Rotation: {0}", zdo.GetRotation().ToDisplayFormat());

            var tag = zdo.GetTag();
            if (!string.IsNullOrEmpty(tag))
            {
                stringBuilder.Append($" Tag: {tag}");
            }
        }

        public bool IsAvailableTo(ZDO zdo) => true;
    }
}