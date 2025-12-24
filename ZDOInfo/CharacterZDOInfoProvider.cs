using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class CharacterZDOInfoProvider : ZDOInfoProviderBase<Character>
    {
        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            stringBuilder.Append($"Level: {zdo.GetInt(ZDOVars.s_level)}");
            var maxHealth = zdo.GetFloat(ZDOVars.s_maxHealth);
            stringBuilder.Append($" Health: {zdo.GetFloat(ZDOVars.s_health, maxHealth).ToDisplayFormat()}/{maxHealth.ToDisplayFormat()}");
            stringBuilder.Append($" Tamed: {zdo.GetBool(ZDOVars.s_tamed)}");

            var name = zdo.GetString(ZDOVars.s_tamedName);
            var nameAuthor = zdo.GetString(ZDOVars.s_tamedNameAuthor);
            if (!string.IsNullOrEmpty(name))
            {
                stringBuilder.AppendFormat(" Name: {0} (author: {1})", name, nameAuthor);
            }
        }
    }
}