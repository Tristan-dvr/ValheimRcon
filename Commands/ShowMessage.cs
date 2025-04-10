namespace ValheimRcon.Commands
{
    internal class ShowMessage : RconCommand
    {
        public override string Command => "showMessage";

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
