using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using ValheimRcon.Core;

namespace ValheimRcon.Tests.Core
{
    [TestFixture]
    public class IpAddressFilterTests
    {
        [SetUp]
        public void SetUp()
        {
            Log.CreateInstance(new MockLogSource());
        }

        #region RefreshFilter Tests

        [Test]
        public void RefreshFilter_WithEmptyLists_ShouldClearBothLists()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "127.0.0.1" }, new[] { "192.168.1.1" });
            filter.RefreshFilter(new List<string>(), new List<string>());

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
        }

        [Test]
        public void RefreshFilter_WithNewLists_ShouldReplaceOldLists()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "127.0.0.1" }, new[] { "192.168.1.1" });
            filter.RefreshFilter(new[] { "10.0.0.1" }, new[] { "172.16.0.1" });

            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        [Test]
        public void RefreshFilter_WithNullLists_ClearAllLists()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(null, null);

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        #endregion

        #region RefreshFilter Parsing Tests

        [Test]
        public void RefreshFilter_WithValidIPStrings_ShouldParseCorrectly()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "127.0.0.1", "192.168.1.1" }, new[] { "10.0.0.1" });

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        [Test]
        public void RefreshFilter_WithValidNetworkStrings_ShouldParseCorrectly()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "192.168.1.0/24" }, new[] { "10.0.0.0/8" });

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.100")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        [Test]
        public void RefreshFilter_WithWhitespaceInStrings_ShouldTrimAndParse()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { " 127.0.0.1 ", "  192.168.1.1  " }, new[] { " 10.0.0.1 " });

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        [Test]
        public void RefreshFilter_WithInvalidBlacklist_ClearBlackList()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new[] { "invalid-ip", "not-an-ip-address" });

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("198.255.0.3")));
        }

        [Test]
        public void RefreshFilter_WithMixedValidAndInvalidWhitelist_ShouldSwitchToLocalhost()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "127.0.0.1", "invalid-ip" }, new List<string>());

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("198.255.0.3")));
        }

        [Test]
        public void RefreshFilter_WithMixedValidAndInvalidBlacklist_ClearBlackList()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new[] { "192.168.1.1", "invalid-ip" });

            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("198.255.0.3")));
        }

        #endregion

        #region IsAllowed Tests - Null Address

        [Test]
        public void IsAllowed_WithNullAddress_ShouldReturnFalse()
        {
            var filter = new IpAddressFilter();
            
            var result = filter.IsAllowed(null);
            
            Assert.IsFalse(result);
        }

        #endregion

        #region IsAllowed Tests - Empty Lists

        [Test]
        public void IsAllowed_WithEmptyLists_ShouldAllowEverything()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new List<string>());
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        [Test]
        public void IsAllowed_WithNoRefresh_ShouldAllowAll()
        {
            var filter = new IpAddressFilter();
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
        }

        #endregion

        #region IsAllowed Tests - Whitelist Only

        [TestCase("127.0.0.1", "127.0.0.1", true)]
        [TestCase("192.168.1.1", "192.168.1.1", true)]
        [TestCase("10.0.0.1", "10.0.0.1", true)]
        [TestCase("127.0.0.1", "192.168.1.1", false)]
        [TestCase("192.168.1.1", "127.0.0.1", false)]
        public void IsAllowed_WithWhitelistSingleIP_ShouldAllowOnlyWhitelistedIPs(string whitelistIP, string testIP, bool expected)
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { whitelistIP }, new List<string>());
            
            var result = filter.IsAllowed(IPAddress.Parse(testIP));
            
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void IsAllowed_WithWhitelistMultipleIPs_ShouldAllowAllWhitelistedIPs()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "127.0.0.1", "192.168.1.1", "10.0.0.1" }, new List<string>());
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("172.16.0.1")));
        }

        [TestCase("192.168.1.0/24", "192.168.1.1", true)]
        [TestCase("192.168.1.0/24", "192.168.1.100", true)]
        [TestCase("192.168.1.0/24", "192.168.1.255", true)]
        [TestCase("192.168.1.0/24", "192.168.0.1", false)]
        [TestCase("192.168.1.0/24", "192.168.2.1", false)]
        [TestCase("10.0.0.0/8", "10.0.0.1", true)]
        [TestCase("10.0.0.0/8", "10.255.255.255", true)]
        [TestCase("10.0.0.0/8", "11.0.0.1", false)]
        public void IsAllowed_WithWhitelistNetwork_ShouldAllowIPsInNetwork(string network, string testIP, bool expected)
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { network }, new List<string>());
            
            var result = filter.IsAllowed(IPAddress.Parse(testIP));
            
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void IsAllowed_WithWhitelistMultipleNetworks_ShouldAllowIPsInAnyNetwork()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "192.168.1.0/24", "10.0.0.0/8" }, new List<string>());
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("172.16.0.1")));
        }

        [Test]
        public void IsAllowed_WithWhitelistMixedIPsAndNetworks_ShouldAllowAll()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "127.0.0.1", "192.168.1.0/24" }, new List<string>());
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.100")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        #endregion

        #region IsAllowed Tests - Blacklist Only

        [TestCase("127.0.0.1", "127.0.0.1", false)]
        [TestCase("192.168.1.1", "192.168.1.1", false)]
        [TestCase("127.0.0.1", "192.168.1.1", true)]
        [TestCase("192.168.1.1", "127.0.0.1", true)]
        public void IsAllowed_WithBlacklistSingleIP_ShouldBlockOnlyBlacklistedIPs(string blacklistIP, string testIP, bool expected)
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new[] { blacklistIP });
            
            var result = filter.IsAllowed(IPAddress.Parse(testIP));
            
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void IsAllowed_WithBlacklistMultipleIPs_ShouldBlockAllBlacklistedIPs()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new[] { "127.0.0.1", "192.168.1.1", "10.0.0.1" });
            
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("172.16.0.1")));
        }

        [TestCase("192.168.1.0/24", "192.168.1.1", false)]
        [TestCase("192.168.1.0/24", "192.168.1.100", false)]
        [TestCase("192.168.1.0/24", "192.168.1.255", false)]
        [TestCase("192.168.1.0/24", "192.168.0.1", true)]
        [TestCase("192.168.1.0/24", "192.168.2.1", true)]
        [TestCase("10.0.0.0/8", "10.0.0.1", false)]
        [TestCase("10.0.0.0/8", "10.255.255.255", false)]
        [TestCase("10.0.0.0/8", "11.0.0.1", true)]
        public void IsAllowed_WithBlacklistNetwork_ShouldBlockIPsInNetwork(string network, string testIP, bool expected)
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new[] { network });
            
            var result = filter.IsAllowed(IPAddress.Parse(testIP));
            
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void IsAllowed_WithBlacklistMultipleNetworks_ShouldBlockIPsInAnyNetwork()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new[] { "192.168.1.0/24", "10.0.0.0/8" });
            
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("172.16.0.1")));
        }

        [Test]
        public void IsAllowed_WithBlacklistMixedIPsAndNetworks_ShouldBlockAll()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new List<string>(), new[] { "127.0.0.1", "192.168.1.0/24" });
            
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.100")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        #endregion

        #region IsAllowed Tests - Whitelist Priority

        [Test]
        public void IsAllowed_WithBothWhitelistAndBlacklist_ShouldPrioritizeBlacklist()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "192.168.1.1" }, new[] { "192.168.1.1" });
            
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
        }

        [Test]
        public void IsAllowed_WithWhitelistAndBlacklist_BlacklistTakesPrecedence()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "192.168.1.0/24" }, new[] { "192.168.1.100" });
            
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.100")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        #endregion

        #region Edge Cases Tests

        [TestCase("127.0.0.1/32")]
        [TestCase("192.168.1.1/32")]
        public void IsAllowed_WithSingleHostNetwork_ShouldWorkCorrectly(string network)
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { network }, new List<string>());
            
            var ip = IPAddress.Parse(network.Split('/')[0]);
            var result = filter.IsAllowed(ip);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void IsAllowed_WithVeryLargeNetwork_ShouldWorkCorrectly()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "0.0.0.0/0" }, new List<string>());
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("10.0.0.1")));
        }

        [Test]
        public void IsAllowed_WithVerySmallNetwork_ShouldWorkCorrectly()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "192.168.1.0/30" }, new List<string>());
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.0")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.2")));
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("192.168.1.3")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.4")));
        }

        [Test]
        public void RefreshFilter_WithDuplicateEntries_ShouldHandleCorrectly()
        {
            var filter = new IpAddressFilter();
            filter.RefreshFilter(new[] { "127.0.0.1", "127.0.0.1" }, new List<string>());
            
            Assert.IsTrue(filter.IsAllowed(IPAddress.Parse("127.0.0.1")));
            Assert.IsFalse(filter.IsAllowed(IPAddress.Parse("192.168.1.1")));
        }

        #endregion
    }
}
