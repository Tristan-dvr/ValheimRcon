using System.Text;

namespace ValheimRcon.Commands
{
    internal class PrintBanlist : RconCommand
    {
        public override string Command => "banlist";

        private StringBuilder stringBuilder = new StringBuilder();

        protected override string OnHandle(CommandArgs args)
        {
            stringBuilder.Clear();

            foreach (var banned in ZNet.instance.m_bannedList.GetList())
            {
                stringBuilder.AppendLine(banned);
            }

            return stringBuilder.ToString();
        }
    }
}
