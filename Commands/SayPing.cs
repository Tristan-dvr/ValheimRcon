using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class SayPing : RconCommand
    {
        public override string Command => "ping";

        public override string Description => "Sends a ping message to all players at the specified coordinates. " +
            "Usage: ping <x> <y> <z>";

        protected override string OnHandle(CommandArgs args)
        {
            var position = args.GetVector3(0);

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,
                "ChatMessage",
                position,
                (int)Talker.Type.Ping,
                Plugin.CommandsUserInfo,
                "");

            return $"Ping sent to {position.ToDisplayFormat()}";
        }
    }
}
