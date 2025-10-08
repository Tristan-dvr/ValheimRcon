namespace ValheimRcon.Commands.Modification
{
    public struct HealthModification : IZdoModification
    {
        private readonly float _health;

        public HealthModification(float health)
        {
            _health = health;
        }

        public void Apply(ZDO zdo)
        {
            zdo.Set(ZDOVars.s_health, _health);
        }
    }
}
