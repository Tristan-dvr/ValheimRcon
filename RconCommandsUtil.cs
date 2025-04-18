﻿using System;
using System.Linq;
using System.Reflection;

namespace ValheimRcon
{
    public static class RconCommandsUtil
    {
        public const int MaxMessageLength = 800;

        public static string Truncate(string message) => message.Length > MaxMessageLength
            ? message.Substring(0, MaxMessageLength)
            : message;

        public static void RegisterAllCommands(Assembly assembly)
        {
            if (RconProxy.Instance == null)
            {
                Log.Error("RconProxy not initialized");
                return;
            }

            Log.Info("Registering rcon commands...");

            var commandInterfaceType = typeof(IRconCommand);
            var commands = assembly
                .GetTypes()
                .Where(t => t != null)
                .Where(t => !t.IsAbstract && t.IsClass)
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .Where(t => t.GetInterfaces().Contains(commandInterfaceType));

            foreach (var type in commands)
                RconProxy.Instance.RegisterCommand(type);
        }
    }
}
