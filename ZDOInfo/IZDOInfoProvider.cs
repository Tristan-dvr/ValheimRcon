using System.Text;

namespace ValheimRcon.ZDOInfo
{
    public interface IZDOInfoProvider
    {
        bool IsAvailableTo(ZDO zdo);
        void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed);
    }
}
