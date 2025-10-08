namespace ValheimRcon.Commands.Search
{
    public struct TagCriteria : ISearchCriteria
    {
        private readonly string _tag;

        public TagCriteria(string tag)
        {
            _tag = tag;
        }

        public bool IsMatch(ZDO zdo) => ZdoUtils.GetTag(zdo) == _tag;
    }
}


