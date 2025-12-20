namespace ValheimRcon.Commands.Search
{
    public struct TagCriteria : ISearchCriteria
    {
        private readonly string _tag;

        public TagCriteria(string tag)
        {
            _tag = tag;
        }

        public bool IsMatch(ZDO zdo) => zdo.GetTag() == _tag;
    }
}


