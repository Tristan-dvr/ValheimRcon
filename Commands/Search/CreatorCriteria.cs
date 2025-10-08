namespace ValheimRcon.Commands.Search
{
    public struct CreatorCriteria : ISearchCriteria
    {
        private readonly long _creatorId;

        public CreatorCriteria(long creatorId)
        {
            _creatorId = creatorId;
        }

        public bool IsMatch(ZDO zdo) => zdo.GetLong(ZDOVars.s_creator) == _creatorId;
    }
}


