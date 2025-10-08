namespace ValheimRcon.Commands
{
    internal class AddGlobalKeyCommand : RconCommand
    {
        public override string Command => "addGlobalKey";

        public override string Description => "Adds a global key to the server. Usage: addGlobalKey <key>";

        protected override string OnHandle(CommandArgs args)
        {
            var key = args.GetString(0);

            ZoneSystem.instance.GlobalKeyAdd(key, true);
            return $"Added global key: {key}";
        }
    }
}
