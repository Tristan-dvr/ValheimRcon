using BepInEx;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    internal class ServerLogs : IRconCommand
    {
        private const int MaxLinesToDisplay = 5;
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
            var startIndex = Math.Max(lines.Length - MaxLinesToDisplay - 1, 0);

            _builder.Clear();
            for (var i = startIndex; i < lines.Length; i++)
            {
                _builder.AppendLine(lines[i]);
            }

            return Task.FromResult(new CommandResult
            {
                Text = _builder.ToString().Trim('\n'),
                AttachedFilePath = targetPath,
            });
        }
    }
}
