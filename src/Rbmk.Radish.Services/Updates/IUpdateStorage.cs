using System.Threading.Tasks;
using Rbmk.Utils.Meta;

namespace Rbmk.Radish.Services.Updates
{
    public interface IUpdateStorage
    {
        Task SaveSkipUpdate(Version version);

        Task<bool> CheckSkipUpdate(Version version);
    }
}