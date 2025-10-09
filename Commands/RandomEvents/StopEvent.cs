using UnityEngine;

namespace ValheimRcon.Commands.RandomEvents
{
    internal class StopEvent : RconCommand
    {
        public override string Command => "stopEvent";

        public override string Description => "Stops the currently active random event.";

        protected override string OnHandle(CommandArgs args)
        {
            RandEventSystem.instance.ResetRandomEvent();
            return "Current random event stopped.";
        }
    }
}
