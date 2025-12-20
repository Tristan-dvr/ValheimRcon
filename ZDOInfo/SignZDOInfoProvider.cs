using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class SignZDOInfoProvider : ZDOInfoProviderBase<Sign>
    {
        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder)
        {
            var text = zdo.GetString(ZDOVars.s_text);
            var author = zdo.GetString(ZDOVars.s_author);
            if (string.IsNullOrEmpty(text))
                return;

            stringBuilder.AppendFormat(" Text: {0} (author: {1})", text, author);
        }
    }
}