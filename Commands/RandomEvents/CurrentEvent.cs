namespace ValheimRcon.Commands.RandomEvents
{
    internal class CurrentEvent : RconCommand
    {
        public override string Command => "currentEvent";

        public override string Description => "Displays the currently active random event";

        protected override string OnHandle(CommandArgs args)
        {
            var ev = RandEventSystem.instance.GetCurrentRandomEvent();
            return ev != null
                ? $"Current event: {ev.m_name} Position: {ev.m_pos.ToDisplayFormat()}"
                : "No active random event.";
        }
    }
}
