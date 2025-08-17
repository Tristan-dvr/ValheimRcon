namespace ValheimRcon.Commands
{
    internal class AddPermitted : RconCommand
    {
        public override string Command => "addPermitted";

        public override string Description => "Adds a player to the permitted list. Usage: addPermitted <steamId>";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_permittedList.Add(steamId);
            return $"{steamId} added to permitted";
        }
    }
}
