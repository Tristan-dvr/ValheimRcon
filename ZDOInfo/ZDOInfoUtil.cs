using System.Text;
using static ItemDrop;

namespace ValheimRcon.ZDOInfo
{
    public static class ZDOInfoUtil
    {
        private static readonly CustomZDOInfoProvider _externalProvider = new CustomZDOInfoProvider();

        private static readonly CustomZDOInfoProvider _globalProvider = new CustomZDOInfoProvider(
            new CommonZDOInfoProvider(),
            new BuildingZDOInfoProvider(),
            new ItemDropZDOInfoProvider(),
            new CharacterZDOInfoProvider(),
            new GuardStoneZDOInfoProvider(),
            new ArmorStandZDOInfoProvider(),
            new ItemStandZDOInfoProvider(),
            new ContainerZDOInfoProvider(),
            new BedZDOInfoProvider(),
            new TombStoneZDOInfoProvider(),
            new SignZDOInfoProvider(),
            new PortalZDOInfoProvider(),
            _externalProvider);

        public static void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed = true)
        {
            _globalProvider.AppendInfo(zdo, stringBuilder, detailed);

            if (!zdo.Persistent)
            {
                stringBuilder.Append(" [NOT PERSISTENT]");
            }
        }

        public static void AppendItemInfo(ItemData item, StringBuilder stringBuilder)
        {
            stringBuilder.AppendFormat("Stack: {0}", item.m_stack);
            stringBuilder.AppendFormat(" Quality: {0}", item.m_quality);

            if (item.m_variant != 0)
                stringBuilder.AppendFormat(" Variant: {0}", item.m_variant);

            if (item.m_crafterID != 0)
                stringBuilder.AppendFormat(" Crafter: {0} ({1})", item.m_crafterName, item.m_crafterID);

            if (item.m_customData.Count > 0)
            {
                stringBuilder.Append($" Data:");
                foreach (var data in item.m_customData)
                {
                    stringBuilder.Append($" '{data.Key}'='{data.Value}'");
                }
            }
        }

        public static void RegisterInfoProvider(IZDOInfoProvider provider)
        {
            _externalProvider.AddProvider(provider);
        }
    }
}
