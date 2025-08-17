using BepInEx;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    internal class ServerLogs : IRconCommand
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public string Command => "logs";

        public string Description => "Get the server logs";

        public Task<CommandResult> HandleCommandAsync(CommandArgs args)
        {
            var sourcePath = Path.Combine(Paths.BepInExRootPath, "LogOutput.log");
            if (!File.Exists(sourcePath)) return Task.FromResult(CommandResult.WithText("No logs"));

            var targetPath = Path.Combine(Paths.CachePath, "LogOutput.log");
            File.Copy(sourcePath, targetPath, true);
            var lines = File.ReadAllLines(targetPath);
            _builder.Clear();
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                _builder.Insert(0, lines[i]);
                _builder.Insert(0, '\n');

                if (_builder.Length > RconCommandsUtil.MaxMessageLength)
                    break;
            }

            return Task.FromResult(new CommandResult
            {
                Text = _builder.ToString().Trim('\n'),
                AttachedFilePath = targetPath,
            });
        }
    }
}
