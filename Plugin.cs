using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Splatform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using ValheimRcon.Commands;
using ValheimRcon.Core;

namespace ValheimRcon
{
    [BepInProcess("valheim_server.exe")]
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "org.tristan.rcon";
        public const string Name = "Valheim Rcon";
        public const string Version = "1.5.1";

        private const int MaxDiscordMessageLength = 1900;
        private const int TruncatedMessageLength = 200;

        public static ConfigEntry<string> DiscordUrl;
        public static ConfigEntry<string> Password;
        public static ConfigEntry<int> Port;
        public static ConfigEntry<string> ServerChatName;

        private static ConfigEntry<string> WhiteListConfig;
        private static ConfigEntry<string> BlackListConfig;

        private static ConfigEntry<string> DiscordSecurityUrl;
        private static ConfigEntry<string> DiscordSecurityReportPrefix;
        private static ConfigEntry<Incident> LogIncidents;

        private DiscordService _discordService;
        private StringBuilder _builder = new StringBuilder();

        public static readonly UserInfo CommandsUserInfo = new UserInfo
        {
            Name = string.Empty,
            UserId = new PlatformUserID("Bot", 0, false),
        };

        public static IpAddressFilter IpFilter = new IpAddressFilter();

        private void Awake()
        {
            Log.CreateInstance(Logger);

            Port = Config.Bind("1. Rcon", "Port", 2458, "Port to receive RCON commands. [Server restart required for update]");
            Password = Config.Bind("1. Rcon", "Password", "", "Password for RCON packages validation. Empty password means plugin will not work! [Server restart required for update]");
            WhiteListConfig = Config.Bind("1. Rcon", "Whitelist IP mask", "", "Comma-separated list of IP addresses or masks (e.g., 192.168.1.0/24, 10.0.0.1)");
            BlackListConfig = Config.Bind("1. Rcon", "Blacklist IP mask", "", "Comma-separated list of IP addresses or masks (e.g., 192.168.1.0/24, 10.0.0.1)");
            DiscordUrl = Config.Bind("2. Discord", "Webhook url", "", "Discord webhook for sending command results");
            ServerChatName = Config.Bind("3. Chat", "Server name", "Server", "Name of server to display messages sent with rcon command");

            DiscordSecurityReportPrefix = Config.Bind("4. Security", "Message prefix", "@here Security alert", "Prefix attached to every security report");
            DiscordSecurityUrl = Config.Bind("4. Security", "Webhook url", "", "Discord webhook for sending security reports");
            LogIncidents = Config.Bind("4. Security", "Incidents",
                Incident.UnauthorizedAccess | Incident.UnexpectedBehaviour, "Incident types will be reported to discord");

            CommandsUserInfo.Name = ServerChatName.Value;

            _discordService = new DiscordService();

            RefreshIpFilter();
            Helper.WatchConfigFileChanges(Config, RefreshIpFilter);

            DontDestroyOnLoad(new GameObject(nameof(RconProxy), typeof(RconProxy)));
            RconProxy.Instance.OnCommandCompleted += SendResultToDiscord;
            RconProxy.Instance.OnSecurityReport += SendReportToDiscord;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            if (string.IsNullOrWhiteSpace(Password.Value))
            {
                Log.Error($"Password is empty. Plugin will not work. Please configure a secure password and restart the server.");
                return;
            }

            RconCommandsUtil.RegisterAllCommands(Assembly.GetExecutingAssembly());
        }

        private void OnDestroy()
        {
            _discordService.Dispose();
        }

        private void SendResultToDiscord(IRconPeer peer, string command, IReadOnlyList<string> args, CommandResult result)
        {
            var url = DiscordUrl.Value;
            if (string.IsNullOrEmpty(url))
                return;

            var fullCommand = $"{command} {string.Join(" ", args)}";
            _builder.Clear();
            _builder.AppendLine($"> {peer.Address} -> {fullCommand}");

            if (_builder.Length > MaxDiscordMessageLength)
            {
                var text = RconCommandsUtil.TruncateMessage(_builder.ToString(), 200);
                _builder.Clear();
                _builder.Append(text);
                _builder.AppendLine("...");
            }

            var availableMessageLength = MaxDiscordMessageLength - _builder.Length;

            if (result.Text.Length > availableMessageLength)
            {
                var truncatedResult = RconCommandsUtil.TruncateMessage(result.Text, TruncatedMessageLength);
                _builder.AppendLine(truncatedResult);
                _builder.Append("*--- message truncated ---*");

                _discordService.SendResult(url,
                    _builder.ToString(),
                    result.AttachedFilePath);

                var tempFilePath = Path.Combine(Paths.CachePath, $"{DateTime.UtcNow.Ticks}.txt");
                FileHelpers.EnsureDirectoryExists(tempFilePath);
                File.WriteAllText(tempFilePath, result.Text);

                _discordService.SendResult(url,
                    "**Full message**",
                    tempFilePath);
            }
            else
            {
                _builder.Append(result.Text);
                _discordService.SendResult(url,
                    _builder.ToString(),
                    result.AttachedFilePath);
            }
        }

        private void SendReportToDiscord(object endPoint, Incident incident, string reason)
        {
            if (!LogIncidents.Value.HasFlag(incident))
                return;

            var url = DiscordSecurityUrl.Value;
            if (string.IsNullOrEmpty(url))
                return;

            _builder.Clear();
            if (!string.IsNullOrEmpty(DiscordSecurityReportPrefix.Value))
            {
                _builder.AppendLine(DiscordSecurityReportPrefix.Value);
            }
            _builder.AppendFormat("[`{0}`] Disconnected for security reasons", endPoint);
            _builder.AppendLine();
            _builder.AppendFormat("**Reason**: {0}", reason);

            _discordService.SendResult(url, _builder.ToString(), null);
        }

        private void RefreshIpFilter()
        {
            var blackList = ParseList(BlackListConfig.Value);
            var whiteList = ParseList(WhiteListConfig.Value);
            IpFilter.RefreshFilter(whiteList, blackList);
            Log.Debug($"IP filter updated {IpFilter}");
        }

        private static IEnumerable<string> ParseList(string config)
        {
            return config.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyFinalizer]
            [HarmonyPatch(typeof(ZNet), nameof(ZNet.UpdatePlayerList))]
            private static void ZNet_UpdatePlayerList(ZNet __instance)
            {
                if (ZNet.TryGetPlayerByPlatformUserID(CommandsUserInfo.UserId, out _) || __instance.m_players.Count == 0)
                    return;

                var name = ServerChatName.Value;
                var playerInfo = new ZNet.PlayerInfo
                {
                    m_name = name,
                    m_userInfo = new ZNet.CrossNetworkUserInfo { m_displayName = name, m_id = CommandsUserInfo.UserId },
                    m_serverAssignedDisplayName = name,
                };
                __instance.m_players.Add(playerInfo);
            }
        }
    }
}