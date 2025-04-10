using BepInEx;
using System.IO;
using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    internal class ServerLogs : IRconCommand
    {
        public string Command => "logs";

        public Task<CommandResult> HandleCommandAsync(CommandArgs args)
        {
            var sourcePath = Path.Combine(Paths.BepInExRootPath, "LogOutput.log");
            if (!File.Exists(sourcePath)) return Task.FromResult(CommandResult.WithText("No logs"));

            var targetPath = Path.Combine(Paths.CachePath, "LogOutput.log");
            File.Copy(sourcePath, targetPath, true);

            return Task.FromResult(new CommandResult
            {
                Text = "Logs:",
                AttachedFilePath = targetPath,
            });
        }
    }
}
