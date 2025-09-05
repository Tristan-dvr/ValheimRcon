namespace ValheimRcon.Commands
{
    public readonly struct ObjectId
    {
        public readonly uint Id;
        public readonly long UserId;

        public ObjectId(uint id, long userId)
        {
            Id = id;
            UserId = userId;
        }
    }
}
