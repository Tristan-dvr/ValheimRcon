using UnityEngine;

namespace ValheimRcon.Commands.Modification
{
    public struct PositionModification : IZdoModification
    {
        private readonly Vector3 _position;

        public PositionModification(Vector3 position)
        {
            _position = position;
        }

        public void Apply(ZDO zdo) => zdo.SetPosition(_position);
    }
}
