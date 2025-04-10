using BepInEx;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    internal class ShowPlayers : IRconCommand
    {
        private static readonly string TempFilePath = Path.Combine(Paths.BepInExRootPath, 
            "Debug", 
            "online_players.txt");

        public string Command => "players";

        private StringBuilder builder = new StringBuilder();

        public ShowPlayers()
        {
            FileHelpers.EnsureDirectoryExists(TempFilePath);
        }

        public Task<CommandResult> HandleCommandAsync(CommandArgs args)
        {
            builder.Clear();
            var online = ZNet.instance.GetPeers().Count;

            foreach (var player in ZNet.instance.GetPeers())
            {
                builder.AppendFormat("{0}:{1} - {2}({3})",
                    player.GetSteamId(),
                    player.m_playerName,
                    player.GetRefPos(),
                    ZoneSystem.GetZone(player.GetRefPos()));

                builder.AppendLine();
            }

            File.WriteAllText(TempFilePath, builder.ToString());

            return Task.FromResult(new CommandResult
            {
                Text = $"Online: {online}\n{GetDisplayResult(builder.ToString())}",
                AttachedFilePath = TempFilePath,
            });
        }

        private static string GetDisplayResult(string result)
        {
            if (result.Length > 100)
            {
                return $"{result.Substring(0, 100)}...";
            }
            else
            {
                return result;
            }
        }
    }
}
