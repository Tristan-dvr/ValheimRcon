using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class SayPing : RconCommand
    {
        public override string Command => "ping";

        protected override string OnHandle(CommandArgs args)
        {
            var position = new Vector3();
            position.x = args.GetInt(0);
            position.y = args.GetInt(1);
            position.z = args.GetInt(2);

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,
                "ChatMessage",
                position,
                (int)Talker.Type.Ping,
                Plugin.CommandsUserInfo,
                "");

            return $"Ping sent to {position}";
        }
    }
}
