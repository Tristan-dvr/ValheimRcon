namespace ValheimRcon.Commands
{
    internal class BanSteamId : RconCommand
    {
        public override string Command => "banSteamId";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_bannedList.Add(steamId);
            return $"{steamId} banned";
        }
    }
}
