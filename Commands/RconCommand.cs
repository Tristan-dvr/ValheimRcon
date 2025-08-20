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

        public abstract string Description { get; }

        public Task<CommandResult> HandleCommandAsync(CommandArgs args)
        {
            return Task.FromResult(new CommandResult
            {
                Text = OnHandle(args).Trim(),
            });
        }

        protected abstract string OnHandle(CommandArgs args);
    }
}
