namespace ValheimRcon.Commands
{
    internal class Ban : RconCommand
    {
        public override string Command => "ban";

        public override string Description => "Ban a user from the server. Usage: ban <playername or steamid>";

        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            ZNet.instance.Ban(user);
            return $"Banned {user}";
        }
    }
}
