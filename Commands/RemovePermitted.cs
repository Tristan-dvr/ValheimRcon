namespace ValheimRcon.Commands
{
    internal class RemovePermitted : RconCommand
    {
        public override string Command => "removePermitted";

        public override string Description => "Removes a player from the permitted list. Usage: removePermitted <steamId>";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_permittedList.Remove(steamId);
            return $"{steamId} removed from permitted";
        }
    }
}
