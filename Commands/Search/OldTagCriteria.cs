namespace ValheimRcon.Commands.Search
{
    public struct OldTagCriteria : ISearchCriteria
    {
        private static readonly int OldTagZdoHash = "tag".GetStableHashCode();
        private readonly string _tag;

        public OldTagCriteria(string tag)
        {
            _tag = tag;
        }

        public bool IsMatch(ZDO zdo) => zdo.GetString(OldTagZdoHash) == _tag;
    }
}


