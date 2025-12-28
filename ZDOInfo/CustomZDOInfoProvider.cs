using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal class CustomZDOInfoProvider : IZDOInfoProvider
    {
        private readonly HashSet<IZDOInfoProvider> _providers = new HashSet<IZDOInfoProvider>();

        public CustomZDOInfoProvider(params IZDOInfoProvider[] providers)
        {
            _providers = providers.ToHashSet();
        }

        public void AddProvider(IZDOInfoProvider provider)
        {
            _providers.Add(provider);
        }

        public void RemoveProvider(IZDOInfoProvider provider)
        {
            _providers.Remove(provider);
        }

        public void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            var hasAny = false;
            foreach (var provider in _providers)
            {
                if (!provider.IsAvailableTo(zdo))
                    continue;

                if (hasAny)
                    stringBuilder.Append(' ');

                provider.AppendInfo(zdo, stringBuilder, detailed);
                hasAny = true;
            }
        }

        public bool IsAvailableTo(ZDO zdo)
        {
            if (_providers.Count == 0)
                return false;

            foreach (var provider in _providers)
            {
                if (provider.IsAvailableTo(zdo))
                    return true;
            }

            return false;
        }
    }
}
