namespace ValheimRcon.Commands
{
    internal class WorldSave : RconCommand
    {
        public override string Command => "save";

        protected override string OnHandle(CommandArgs args)
        {
            ZNet.instance.Save(false);
            return "World save started";
        }
    }
}
