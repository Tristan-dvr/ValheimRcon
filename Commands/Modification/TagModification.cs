namespace ValheimRcon.Commands.Modification
{
    public struct TagModification : IZdoModification
    {
        private readonly string _tag;

        public TagModification(string tag)
        {
            _tag = tag;
        }

        public void Apply(ZDO zdo) => zdo.SetTag(_tag);
    }
}
