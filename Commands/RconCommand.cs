using BepInEx;
using System.IO;
using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    public abstract class RconCommand : IRconCommand
    {
        private static readonly string ResultFilePath = Path.Combine(Paths.CachePath, "result.txt");

        public RconCommand()
        {
            FileHelpers.EnsureDirectoryExists(ResultFilePath);
        }

        public abstract string Command { get; }

        public Task<CommandResult> HandleCommandAsync(CommandArgs args)
        {
            var result = OnHandle(args).Trim();
            var filePath = string.Empty;
            if (result.Length > RconCommandsUtil.MaxMessageLength)
            {
                File.WriteAllText(ResultFilePath, result);
                filePath = ResultFilePath;
                result = RconCommandsUtil.Truncate(result);
            }

            return Task.FromResult(new CommandResult
            {
                Text = OnHandle(args),
                AttachedFilePath = filePath,
            });
        }

        protected abstract string OnHandle(CommandArgs args);
    }
}
