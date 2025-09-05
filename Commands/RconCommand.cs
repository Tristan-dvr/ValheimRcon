using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    public abstract class RconCommand : IRconCommand
    {
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
