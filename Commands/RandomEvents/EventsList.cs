using System.Text;

namespace ValheimRcon.Commands.RandomEvents
{
    internal class EventsList : RconCommand
    {
        public override string Command => "eventsList";

        public override string Description => "Prints all available random events";

        private readonly StringBuilder _builder = new StringBuilder();

        protected override string OnHandle(CommandArgs args)
        {
            _builder.Clear();
            _builder.AppendLine("Available random events:");
            var first = true;
            foreach (var ev in RandEventSystem.instance.m_events)
            {
                if (!first)
                    _builder.Append(", ");
                _builder.Append(ev.m_name);
                first = false;
            }
            return _builder.ToString();
        }
    }
}
