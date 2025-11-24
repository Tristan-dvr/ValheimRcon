using LukeSkywalker.IPNetwork;
using System;
using System.Collections.Generic;
using System.Net;

namespace ValheimRcon.Core
{
    public class IpAddressFilter
    {
        private List<IPNetwork> _whiteList = new List<IPNetwork>();
        private List<IPNetwork> _blackList = new List<IPNetwork>();

        public void RefreshFilter(IEnumerable<string> whiteList, IEnumerable<string> blackList)
        {
            _whiteList.Clear();
            _blackList.Clear();

            if (ParseConfigs(whiteList, out var whiteListNetworks))
            {
                _whiteList.AddRange(whiteListNetworks);
            }
            else
            {
                _whiteList.Add(IPNetwork.Parse("127.0.0.1"));
            }
            ParseConfigs(blackList, out var blacklistNetworks);
            _blackList.AddRange(blacklistNetworks);
        }

        public bool IsAllowed(IPAddress address)
        {
            if (address == null)
                return false;

            if (_blackList.Count > 0 && IsInList(address, _blackList))
            {
                return false;
            }

            return _whiteList.Count == 0
                || IsInList(address, _whiteList);
        }

        private static bool IsInList(IPAddress address, IReadOnlyCollection<IPNetwork> list)
        {
            foreach (var entry in list)
            {
                if (IPNetwork.Contains(entry, address))
                    return true;
            }

            return false;
        }

        private static bool ParseConfigs(IEnumerable<string> configs, out IReadOnlyCollection<IPNetwork> validNetworks)
        {
            var networks = new List<IPNetwork>();
            validNetworks = networks;

            if (configs == null)
                return true;

            var allParsed = true;
            foreach (var config in configs)
            {
                try
                {
                    var network = Parse(config.Trim());
                    networks.Add(network);
                }
                catch (Exception e)
                {
                    Log.Error($"Cannot parse config {config} - {e}");
                }
            }
            return allParsed;
        }

        private static IPNetwork Parse(string config)
        {
            var parts = config.Split('/');
            if (parts.Length == 2)
            {
                var network = parts[0];
                var cidr = byte.Parse(parts[1]);
                return IPNetwork.Parse(network, cidr);
            }
            else if (parts.Length == 1)
            {
                return IPNetwork.Parse(config, 32);
            }
            else
            {
                throw new ArgumentException($"Invalid network {config}");
            }
        }

        public override string ToString()
        {
            return $"{nameof(IpAddressFilter)} - whitelist: {string.Join(",", _whiteList)} blacklist: {string.Join(",", _blackList)}";
        }
    }
}

