using System.Linq;
using ValheimRcon.Commands.Search;

namespace ValheimRcon.Commands.Container
{
    public abstract class ContainerRconCommand : RconCommand
    {
        protected override string OnHandle(CommandArgs args)
        {
            var id = args.GetObjectId(0);
            var idCriteria = new IdCriteria(id);
            var zdo = ZDOMan.instance.m_objectsByID.Values.FirstOrDefault(idCriteria.IsMatch);

            if (zdo == null)
            {
                return "No object found with the specified ID.";
            }

            var prefabHash = zdo.GetPrefab();
            var prefabName = zdo.GetPrefabName();
            var prefab = ZNetScene.instance.GetPrefab(prefabHash);
            
            if (prefab == null)
            {
                return $"Failed to load prefab: {prefabName}";
            }

            var containerComponent = prefab.GetComponentInChildren<global::Container>();
            if (containerComponent == null)
            {
                return $"Object {prefabName} is not a container.";
            }

            var itemsData = zdo.GetString(ZDOVars.s_items, "");
            var inventory = new Inventory(containerComponent.m_name, containerComponent.m_bkg, containerComponent.m_width, containerComponent.m_height);
            
            if (!string.IsNullOrEmpty(itemsData))
            {
                var package = new ZPackage(itemsData);
                inventory.Load(package);
            }

            return HandleInventory(args, zdo, inventory, prefabName);
        }

        protected abstract string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName);

        protected void SaveInventory(ZDO zdo, Inventory inventory)
        {
            var package = new ZPackage();
            inventory.Save(package);
            zdo.Set(ZDOVars.s_items, package.GetBase64());
            zdo.SetZdoModified();
        }
    }
}
