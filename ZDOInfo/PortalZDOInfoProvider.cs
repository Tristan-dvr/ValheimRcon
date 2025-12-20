using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class PortalZDOInfoProvider : ZDOInfoProviderBase<TeleportWorld>
    {
        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder)
        {
            var portalTag = zdo.GetString(ZDOVars.s_tag);
            var author = zdo.GetString(ZDOVars.s_tagauthor);

            stringBuilder.AppendFormat(" Portal tag: {0} (author {1})", portalTag, author);
        }
    }
}
