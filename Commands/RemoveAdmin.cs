namespace ValheimRcon.Commands
{
    internal class RemoveAdmin : RconCommand
    {
        public override string Command => "removeAdmin";

        public override string Description => "Removes a player from the admin list. Usage: removeAdmin <steamId>";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_adminList.Remove(steamId);
            return $"{steamId} removed from admins";
        }
    }
}
