namespace ValheimRcon.Commands
{
    internal class Unban : RconCommand
    {
        public override string Command => "unban";

        protected override string OnHandle(CommandArgs args)
        {
            var user = args.GetString(0);
            ZNet.instance.Unban(user);
            return $"{user} unbanned";
        }
    }
}
