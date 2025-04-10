namespace ValheimRcon.Commands
{
    internal class RemovePermitted : RconCommand
    {
        public override string Command => "removePermitted";

        protected override string OnHandle(CommandArgs args)
        {
            var steamId = args.GetString(0);
            ZNet.instance.m_permittedList.Remove(steamId);
            return $"{steamId} removed from permitted";
        }
    }
}
