using System.Text;

namespace ValheimRcon.Commands
{
    internal class PrintPermitlist : RconCommand
    {
        private StringBuilder stringBuilder = new StringBuilder();

        public override string Command => "permitted";

        protected override string OnHandle(CommandArgs args)
        {
            stringBuilder.Clear();

            foreach (var banned in ZNet.instance.m_permittedList.GetList())
            {
                stringBuilder.AppendLine(banned);
            }

            return stringBuilder.ToString();
        }
    }
}
