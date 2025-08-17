using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class SayChat : RconCommand
    {
        public override string Command => "say";

        public override string Description => "Sends a message to the chat as a shout. Usage: say <message>";

        protected override string OnHandle(CommandArgs args)
        {
            var text = args.ToString();

            if (!ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out var location))
                location = new Vector3(0, 30, 0);

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,
                "ChatMessage",
                location,
                (int)Talker.Type.Shout,
                Plugin.CommandsUserInfo,
                text);

            return $"Sent to chat - {text}";
        }
    }
}
