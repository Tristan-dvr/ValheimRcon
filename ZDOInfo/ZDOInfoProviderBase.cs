using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ValheimRcon.ZDOInfo
{
    internal abstract class ZDOInfoProviderBase<T> : IZDOInfoProvider
        where T : Component
    {
        private readonly Dictionary<int, bool> _prefabs = new Dictionary<int, bool>();

        public virtual bool IsAvailableTo(ZDO zdo)
        {
            var prefabHash = zdo.GetPrefab();
            if (_prefabs.TryGetValue(prefabHash, out var available))
                return available;

            var prefab = ZNetScene.instance.GetPrefab(prefabHash);
            available = prefab != null && prefab.GetComponentInChildren<T>() != null;
            _prefabs[prefabHash] = available;
            return available;
        }

        public abstract void AppendInfo(ZDO zdo, StringBuilder stringBuilder);
    }
}
