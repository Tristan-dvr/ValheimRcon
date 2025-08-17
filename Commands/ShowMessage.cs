namespace ValheimRcon.Commands
{
    internal class ShowMessage : RconCommand
    {
        public override string Command => "showMessage";

        public override string Description => "Displays a message in the center of the screen for all players. " +
            "Usage: showMessage <message>";

        protected override string OnHandle(CommandArgs args)
        {
            var text = args.ToString();

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, 
                "ShowMessage", 
                (int)MessageHud.MessageType.Center, 
                text);

            return $"Message sent - {text}";
        }
    }
}
