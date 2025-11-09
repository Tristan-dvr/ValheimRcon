namespace ValheimRcon.Commands.Container
{
    internal class ClearContainerInventory : ContainerRconCommand
    {
        public override string Command => "clearContainer";

        public override string Description => "Clears all items from a container's inventory. " +
            "Usage: clearContainer <id:userid> " +
            "-force";

        protected override string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName)
        {
            var optionalArgs = args.GetOptionalArguments();
            var force = false;
            
            foreach (var index in optionalArgs)
            {
                if (args.GetString(index) == "-force")
                {
                    force = true;
                    break;
                }
            }


            if (!ContainerUtils.ValidateContainerModification(zdo, prefabName, force, out var validationError))
            {
                return validationError;
            }

            inventory.RemoveAll();
            SaveInventory(zdo, inventory);
            return $"Cleared inventory of {prefabName}";
        }
    }
}