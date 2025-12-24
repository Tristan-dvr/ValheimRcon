namespace ValheimRcon.Commands.Modification
{
    public struct PrefabModification : IZdoModification
    {
        private readonly string _prefabName;

        public PrefabModification(string prefabName)
        {
            _prefabName = prefabName;
        }

        public void Apply(ZDO zdo)
        {
            zdo.SetPrefab(_prefabName.GetStableHashCode());
        }
    }
}
