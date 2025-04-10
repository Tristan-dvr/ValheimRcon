using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    public abstract class RconCommand : IRconCommand
    {
        public abstract string Command { get; }

        public Task<CommandResult> HandleCommandAsync(CommandArgs args)
        {
            return Task.FromResult(new CommandResult { Text = OnHandle(args) });
        }

        protected abstract string OnHandle(CommandArgs args);
    }
}
