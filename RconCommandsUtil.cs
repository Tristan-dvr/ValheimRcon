using System;
using System.Linq;
using System.Reflection;
using System.Text;
using ValheimRcon.Commands;

namespace ValheimRcon
{
    public static class RconCommandsUtil
    {
        public static string TruncateMessage(string message, int maxLength)
        {
            if (message.Length <= maxLength)
                return message;
            return message.Substring(0, maxLength) + "...";
        }

        public static string TruncateMessageByBytes(string message, int maxBytes)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            var bytes = Encoding.UTF8.GetBytes(message);
            if (bytes.Length <= maxBytes)
                return message;

            var result = new StringBuilder();
            var currentBytes = 0;
            
            for (int i = 0; i < message.Length; i++)
            {
                var charBytes = Encoding.UTF8.GetByteCount(message[i].ToString());
                if (currentBytes + charBytes > maxBytes)
                    break;
                    
                result.Append(message[i]);
                currentBytes += charBytes;
            }
            
            return result.ToString();
        }

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
                .Where(t => t.GetInterfaces().Contains(commandInterfaceType))
                .Where(t => t.GetCustomAttribute<ExcludeAttribute>() == null);

            foreach (var type in commands)
                RconProxy.Instance.RegisterCommand(type);
        }
    }
}
