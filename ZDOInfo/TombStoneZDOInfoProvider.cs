using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class TombStoneZDOInfoProvider : ZDOInfoProviderBase<TombStone>
    {
        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder)
        {
            var ownerName = zdo.GetString(ZDOVars.s_ownerName);
            var owner = zdo.GetLong(ZDOVars.s_owner);
            stringBuilder.AppendFormat(" Tombstone: {0}({1})", ownerName, owner);
        }
    }
}