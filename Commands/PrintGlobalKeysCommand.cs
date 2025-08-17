using System.Text;

namespace ValheimRcon.Commands
{
    internal class PrintGlobalKeysCommand : RconCommand
    {
        public override string Command => "globalKeys";
        public override string Description => "Prints all global keys and their values.";

        protected override string OnHandle(CommandArgs args)
        {
            var keys = ZoneSystem.instance.GetGlobalKeys();
            var values = ZoneSystem.instance.m_globalKeysValues;
            var result = new StringBuilder("Global Keys:\n");
            foreach (var key in keys)
            {
                result.Append(key);
                if (values.TryGetValue(key, out var value))
                {
                    result.Append($" : {value}");
                }
                result.AppendLine();
            }
            return result.ToString().Trim();
        }
    }
}
