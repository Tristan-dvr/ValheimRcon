namespace ValheimRcon.Commands
{
    internal class Unban : RconCommand
    {
        public override string Command => "unban";

        public override string Description => "Unban a user from the server. Usage: unban <playername or steamid>";

        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            ZNet.instance.Unban(user);
            return $"{user} unbanned";
        }
    }
}
