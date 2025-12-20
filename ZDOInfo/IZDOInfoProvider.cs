using System.Text;

namespace ValheimRcon.ZDOInfo
{
    internal interface IZDOInfoProvider
    {
        bool IsAvailableTo(ZDO zdo);
        void AppendInfo(ZDO zdo, StringBuilder stringBuilder);
    }
}
