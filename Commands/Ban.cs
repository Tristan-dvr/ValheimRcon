namespace ValheimRcon.Commands
{
    internal class Ban : RconCommand
    {
        public override string Command => "ban";

        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            ZNet.instance.Ban(user);
            return $"Banned {user}";
        }
    }
}
