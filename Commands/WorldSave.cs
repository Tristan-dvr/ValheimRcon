namespace ValheimRcon.Commands
{
    internal class WorldSave : RconCommand
    {
        public override string Command => "save";

        public override string Description => "Save the current world state.";

        protected override string OnHandle(CommandArgs args)
        {
            ZNet.instance.Save(false);
            return "World save started";
        }
    }
}
