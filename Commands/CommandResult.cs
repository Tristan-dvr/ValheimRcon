namespace ValheimRcon.Commands
{
    public struct CommandResult
    {
        public string Text;
        public string AttachedFilePath;

        public static CommandResult WithText(string text) => new CommandResult { Text = text };
    }
}
