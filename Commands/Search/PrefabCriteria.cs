namespace ValheimRcon.Commands.Search
{
    public struct PrefabCriteria : ISearchCriteria
    {
        private readonly int _prefabHash;

        public PrefabCriteria(string prefab)
        {
            _prefabHash = prefab.GetStableHashCode();
        }

        public bool IsMatch(ZDO zdo) => zdo.GetPrefab() == _prefabHash;
    }
}
