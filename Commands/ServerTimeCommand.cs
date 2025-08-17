using System;

namespace ValheimRcon.Commands
{
    internal class ServerTimeCommand : RconCommand
    {
        public override string Command => "time";

        public override string Description => "Get the current server time and day.";

        protected override string OnHandle(CommandArgs args)
        {
            var time = ZNet.instance.GetTimeSeconds();
            var day = EnvMan.instance.GetCurrentDay();
            return $"Current server time: {time} sec. Day: {day}";
        }
    }
}
