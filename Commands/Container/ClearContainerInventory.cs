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
            var force = false;
            
            foreach (var (index, argument) in args.GetOptionalArguments())
            {
                switch (argument)
                {
                    case "-force":
                        force = true;
                        break;
                    default:
                        return $"Unknown argument {argument}";
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