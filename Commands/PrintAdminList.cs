using System.Text;

namespace ValheimRcon.Commands
{
    internal class PrintAdminList : RconCommand
    {
        public override string Command => "adminlist";

        public override string Description => "Prints the list of admins on the server.";


        private StringBuilder stringBuilder = new StringBuilder();

        protected override string OnHandle(CommandArgs args)
        {
            stringBuilder.Clear();

            foreach (var banned in ZNet.instance.m_adminList.GetList())
            {
                stringBuilder.AppendLine(banned);
            }

            return stringBuilder.ToString();
        }
    }
}
