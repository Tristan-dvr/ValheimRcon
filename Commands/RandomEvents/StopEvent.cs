using UnityEngine;

namespace ValheimRcon.Commands.RandomEvents
{
    internal class StopEvent : RconCommand
    {
        public override string Command => "stopEvent";

        public override string Description => "Stops the currently active random event.";

        protected override string OnHandle(CommandArgs args)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,
                "SetEvent",
                string.Empty,
                0f,
                Vector3.zero);
            return "Current random event stopped.";
        }
    }
}
