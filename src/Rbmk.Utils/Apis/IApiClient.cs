using System.Threading.Tasks;
using Rbmk.Utils.Apis.Objects;

namespace Rbmk.Utils.Apis
{
    public interface IApiClient
    {
        Task<ApiResult<Api.ReleaseList>> GetReleases(string product);
    }
}