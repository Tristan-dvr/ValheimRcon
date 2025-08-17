namespace ValheimRcon.Commands
{
    internal class InvokeConsoleCommand : RconCommand
    {
        public override string Command => "consoleCommand";

        public override string Description => "Executes a console command on the server. Usage: consoleCommand <command>";

        protected override string OnHandle(CommandArgs args)
        {
            var command = string.Join(" ", args.Arguments);
            if (string.IsNullOrWhiteSpace(command))
            {
                return "No command provided.";
            }

            Console.instance.TryRunCommand(command, false, true);
            return $"Command '{command}' executed.";
        }
    }
}
