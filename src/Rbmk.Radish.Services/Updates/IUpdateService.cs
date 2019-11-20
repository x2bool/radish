using System.Threading.Tasks;
using Rbmk.Utils.Meta;

namespace Rbmk.Radish.Services.Updates
{
    public interface IUpdateService
    {
        Task<UpdateInfo> CheckUpdate();

        Task SkipUpdate(Version version);
    }
}