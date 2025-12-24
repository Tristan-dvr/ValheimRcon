using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class GuardStoneZDOInfoProvider : ZDOInfoProviderBase<PrivateArea>
    {
        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            stringBuilder.Append($"Enabled: {zdo.GetBool(ZDOVars.s_enabled)}");
            stringBuilder.Append($" Owner: {zdo.GetString(ZDOVars.s_creatorName)}");
            stringBuilder.Append($" Permitted:");

            var permittedPlayers = GetPermittedPlayers(zdo);
            if (!permittedPlayers.Any())
            {
                stringBuilder.Append(" <empty>");
                return;
            }

            foreach (var player in permittedPlayers)
            {
                stringBuilder.Append($" {player}");
            }
        }

        private static IEnumerable<(long Id, string Name)> GetPermittedPlayers(ZDO zdo)
        {
            var count = zdo.GetInt(ZDOVars.s_permitted, 0);
            if (count <= 0)
            {
                yield break;
            }

            for (int i = 0; i < count; i++)
            {
                var id = zdo.GetLong($"pu_id{i}");
                var name = zdo.GetString($"pu_name{i}");
                yield return (id, name);
            }
        }
    }
}