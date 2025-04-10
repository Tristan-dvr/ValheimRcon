namespace ValheimRcon.Commands
{
    internal class RemoveAdmin : RconCommand
    {
        public override string Command => "removeAdmin";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_adminList.Remove(steamId);
            return $"{steamId} removed from admins";
        }
    }
}
