namespace ValheimRcon.Commands.Search
{
    public struct ZoneCriteria : ISearchCriteria
    {
        private Vector2i _zone;

        public ZoneCriteria(Vector2i zone)
        {
            _zone = zone;
        }

        public bool IsMatch(ZDO zdo)
        {
            var position = zdo.GetPosition();
            var zone = ZoneSystem.GetZone(position);
            return zone == _zone;
        }
    }
}
