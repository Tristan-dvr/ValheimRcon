namespace ValheimRcon.Commands.Players
{
    internal class Damage : PlayerRconCommand
    {
        public override string Command => "damage";

        public override string Description => "Damage a player by a specified amount. Usage: damage <steamid> <amount>";

        protected override string OnHandle(ZNetPeer peer, ZDO zdo, CommandArgs args)
        {
            var hitData = new HitData()
            {
                m_blockable = false,
                m_dodgeable = false,
                m_ignorePVP = true,
                m_hitType = HitData.HitType.Undefined,
                m_damage = new HitData.DamageTypes()
                {
                    m_damage = args.GetInt(1),
                },
            };
            peer.InvokeRoutedRpcToZdo("RPC_Damage", hitData);
            return $"{peer.GetPlayerInfo()} took {hitData.m_damage.m_damage.ToDisplayFormat()}hp damage";
        }
    }
}
