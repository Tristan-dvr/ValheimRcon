using System.Collections.Generic;
using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class BuildingZDOInfoProvider : IZDOInfoProvider
    {
        private readonly Dictionary<int, bool> _prefabs = new Dictionary<int, bool>();
        private readonly Dictionary<int, float> _maxHealth = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _maxSupport = new Dictionary<int, float>();

        public void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            stringBuilder.Append($"Creator: {zdo.GetLong(ZDOVars.s_creator)}");
            var maxHealth = _maxHealth.TryGetValue(zdo.GetPrefab(), out var health) ? health : 0f;
            stringBuilder.Append($" Health: {zdo.GetFloat(ZDOVars.s_health, maxHealth).ToDisplayFormat()}");

            if (!detailed)
                return;

            var maxSupport = _maxSupport.TryGetValue(zdo.GetPrefab(), out var support) ? support : 0f;
            stringBuilder.Append($" Support: {zdo.GetFloat(ZDOVars.s_support, maxSupport).ToDisplayFormat()}");
        }

        public bool IsAvailableTo(ZDO zdo)
        {
            var prefabHash = zdo.GetPrefab();
            if (_prefabs.TryGetValue(prefabHash, out var available))
                return available;

            var prefab = ZNetScene.instance.GetPrefab(prefabHash);
            var wearNTear = prefab != null
                ? prefab.GetComponentInChildren<WearNTear>()
                : null;

            available = wearNTear != null;
            _prefabs[prefabHash] = available;
            if (!available)
                return false;

            _maxHealth[prefabHash] = wearNTear.m_health;
            _maxSupport[prefabHash] = wearNTear.m_support;
            return true;
        }
    }
}
