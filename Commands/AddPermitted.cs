namespace ValheimRcon.Commands
{
    internal class AddPermitted : RconCommand
    {
        public override string Command => "addPermitted";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_permittedList.Add(steamId);
            return $"{steamId} added to permitted";
        }
    }
}
