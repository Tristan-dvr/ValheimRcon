using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class BedZDOInfoProvider : ZDOInfoProviderBase<Bed>
    {
        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder)
        {
            var ownerName = zdo.GetString(ZDOVars.s_ownerName);
            var owner = zdo.GetLong(ZDOVars.s_owner);
            stringBuilder.Append(" Bed: ");
            if (string.IsNullOrEmpty(ownerName))
            {
                stringBuilder.Append("<not claimed>");
            }
            else
            {
                stringBuilder.AppendFormat("{0}({1})", ownerName, owner);
            }
        }
    }
}