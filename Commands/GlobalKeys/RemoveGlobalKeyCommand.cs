namespace ValheimRcon.Commands.GlobalKeys
{
    internal class RemoveGlobalKeyCommand : RconCommand
    {
        public override string Command => "removeGlobalKey";

        public override string Description => "Removes a global key from the server. Usage: removeGlobalKey <key>";

        protected override string OnHandle(CommandArgs args)
        {
            var key = args.GetString(0);

            ZoneSystem.instance.GlobalKeyRemove(key, true);
            return $"Global key '{key}' removed successfully.";
        }
    }
}
