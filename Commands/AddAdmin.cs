namespace ValheimRcon.Commands
{
    internal class AddAdmin : RconCommand
    {
        public override string Command => "addAdmin";

        public override string Description => "Adds a player to the admin list. Usage: addAdmin <steamId>";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_adminList.Add(steamId);
            return $"{steamId} is admin now";
        }
    }
}
