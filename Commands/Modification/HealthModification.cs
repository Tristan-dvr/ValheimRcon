using System.Collections.Generic;

namespace ValheimRcon.Commands.Modification
{
    public struct HealthModification : IZdoModification
    {
        private static readonly Dictionary<int, bool> _buildings = new Dictionary<int, bool>();

        private readonly float _health;

        public HealthModification(float health)
        {
            _health = health;
        }

        public void Apply(ZDO zdo)
        {
            zdo.Set(ZDOVars.s_health, _health);

            if (IsBuilding(zdo))
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, zdo.m_uid, "RPC_HealthChanged", _health);
            }
        }

        private static bool IsBuilding(ZDO zdo)
        {
            var prefabHash = zdo.GetPrefab();
            if (_buildings.TryGetValue(prefabHash, out var building))
                return building;

            var prefab = ZNetScene.instance.GetPrefab(prefabHash);
            building = prefab != null && prefab.TryGetComponent<WearNTear>(out _);
            _buildings[prefabHash] = building;
            return building;
        }
    }
}
