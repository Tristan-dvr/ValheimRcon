using UnityEngine;

namespace ValheimRcon.Commands.Search
{
    public struct NearCriteria : ISearchCriteria
    {
        private readonly Vector3 _center;
        private readonly float _radius;

        public NearCriteria(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }

        public bool IsMatch(ZDO zdo)
        {
            var p = zdo.GetPosition();
            return p.x < _center.x + _radius
                && p.x > _center.x - _radius
                && p.y < _center.y + _radius
                && p.y > _center.y - _radius
                && p.z < _center.z + _radius
                && p.z > _center.z - _radius;
        }
    }
}


