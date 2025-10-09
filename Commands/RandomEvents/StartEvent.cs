namespace ValheimRcon.Commands.RandomEvents
{
    internal class StartEvent : RconCommand
    {
        public override string Command => "startEvent";

        public override string Description => "Starts a random event. Usage: startEvent <event_name> <x> <y> <z>";

        protected override string OnHandle(CommandArgs args)
        {
            var eventName = args.GetString(0);
            var position = args.GetVector3(1);
            var ev = RandEventSystem.instance.GetEvent(eventName);
            if (ev == null)
                return $"Event '{eventName}' not found.";

            RandEventSystem.instance.SetRandomEvent(ev, position);
            return $"Event '{eventName}' started at {position.ToDisplayFormat()}";
        }
    }
}
