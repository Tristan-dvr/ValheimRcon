namespace ValheimRcon.Commands.Container
{
    public static class ContainerUtils
    {
        public static bool ValidateContainerModification(ZDO zdo, string prefabName, bool force, out string error)
        {
            if (!zdo.Persistent)
            {
                error = "Object is not persistent and cannot be modified.";
                return false;
            }

            if (!ZdoUtils.CanModifyZdo(zdo))
            {
                error = "Object cannot be modified.";
                return false;
            }

            var isInUse = zdo.GetInt(ZDOVars.s_inUse, 0) == 1;
            if (isInUse)
            {
                error = $"Container {prefabName} is currently in use and cannot be modified.";
                return false;
            }

            var owner = zdo.GetOwner();
            var ownerPlayer = ZNet.instance.GetPeer(owner);
            if (ownerPlayer != null)
            {
                if (!force)
                {
                    error = $"Container {prefabName} is owned by an online player {ownerPlayer.GetPlayerInfo()} and cannot be modified. Use -force to override.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }
    }
}

