using System.Threading.Tasks;
using ValheimRcon.Commands;

namespace ValheimRcon
{
    public interface IRconCommand
    {
        string Command { get; }
        Task<CommandResult> HandleCommandAsync(CommandArgs args);
    }
}
