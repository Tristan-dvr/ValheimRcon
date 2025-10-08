using UnityEngine;

namespace ValheimRcon.Commands.Modification
{
    public struct RotationModification : IZdoModification
    {
        private readonly Vector3 _rotation;

        public RotationModification(Vector3 rotation)
        {
            _rotation = rotation;
        }

        public void Apply(ZDO zdo) => zdo.SetRotation(Quaternion.Euler(_rotation));
    }
}
