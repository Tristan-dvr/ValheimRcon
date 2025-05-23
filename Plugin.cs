﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Splatform;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using ValheimRcon.Commands;

namespace ValheimRcon
{
    [BepInProcess("valheim_server.exe")]
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "org.tristan.rcon";
        public const string Name = "Valheim Rcon";
        public const string Version = "1.0.1";

        public static ConfigEntry<string> DiscordUrl;
        public static ConfigEntry<string> Password;
        public static ConfigEntry<int> Port;
        public static ConfigEntry<string> ServerChatName;

        private string _logsPath;
        private DiscordService _discordService;
        private StringBuilder _builder = new StringBuilder();

        public static readonly UserInfo CommandsUserInfo = new UserInfo
        {
            Name = string.Empty,
            UserId = new PlatformUserID("Bot", 0, false),
        };

        private void Awake()
        {
            Log.CreateInstance(Logger);
            _logsPath = Path.Combine(Paths.BepInExRootPath, "rcon_logs.log");

            Port = Config.Bind("1. Rcon", "Port", 2458, "Port to receive RCON commands");
            Password = Config.Bind("1. Rcon", "Password", System.Guid.NewGuid().ToString(), "Password for RCON packages validation");
            DiscordUrl = Config.Bind("2. Discord", "Webhook url", "", "Discord webhook for sending command results");
            ServerChatName = Config.Bind("3. Chat", "Server name", "Server", "Name of server to display messages sent with rcon command");

            CommandsUserInfo.Name = ServerChatName.Value;

            _discordService = new DiscordService(DiscordUrl.Value);

            DontDestroyOnLoad(new GameObject(nameof(RconProxy), typeof(RconProxy)));
            RconProxy.Instance.OnCommandCompleted += OnCommandCompleted;

            RconCommandsUtil.RegisterAllCommands(Assembly.GetExecutingAssembly());

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnDestroy()
        {
            _discordService.Dispose();
        }

        private void OnCommandCompleted(string command, IReadOnlyList<string> args, CommandResult result)
        {
            _builder.Clear();
            _builder.AppendFormat("Command {0} {1}", command, string.Join(", ", args));
            _builder.AppendLine();
            _builder.Append(result.Text);

            File.AppendAllText(_logsPath, _builder.ToString());

            if (string.IsNullOrEmpty(DiscordUrl.Value))
                return;

            _discordService.SendResult(_builder.ToString(), result.AttachedFilePath);
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