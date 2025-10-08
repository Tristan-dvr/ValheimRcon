namespace ValheimRcon.Commands.Search
{
    public struct IdCriteria : ISearchCriteria
    {
        private readonly uint _id;
        private readonly long _userId;

        public IdCriteria(ObjectId objectId)
        {
            _id = objectId.Id;
            _userId = objectId.UserId;
        }

        public bool IsMatch(ZDO zdo) => zdo.m_uid.ID == _id && zdo.m_uid.UserID == _userId;
    }
}


