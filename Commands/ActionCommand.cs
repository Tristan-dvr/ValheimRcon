using System;
using System.Threading.Tasks;

namespace ValheimRcon.Commands
{
    internal class ActionCommand : IRconCommand
    {
        public string Command { get; }

        public string Description { get; }

        private readonly Func<CommandArgs, CommandResult> _execute;

        public ActionCommand(string command, string description, Func<CommandArgs, CommandResult> execute)
        {
            Command = command;
            _execute = execute;
            Description = description;
        }

        public Task<CommandResult> HandleCommandAsync(CommandArgs args)
        {
            return Task.FromResult(_execute(args));
        }
    }
}
