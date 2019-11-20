using System.Linq;
using System.Threading.Tasks;
using Rbmk.Utils.Apis;
using Rbmk.Utils.Meta;

namespace Rbmk.Radish.Services.Updates
{
    public class UpdateService : IUpdateService
    {
        private readonly IApplicationInfo _applicationInfo;
        private readonly IApiClient _apiClient;
        private readonly IUpdateStorage _updateStorage;

        public UpdateService(
            IApplicationInfo applicationInfo,
            IApiClient apiClient,
            IUpdateStorage updateStorage)
        {
            _applicationInfo = applicationInfo;
            _apiClient = apiClient;
            _updateStorage = updateStorage;
        }
        
        public async Task<UpdateInfo> CheckUpdate()
        {
            var result = await _apiClient.GetReleases(_applicationInfo.Name);

            var update = result?.Data?.Releases?.Select(r => new UpdateInfo
                {
                    Version = Version.Parse(r.Version),
                    DownloadUrl = r.DownloadUrl
                })
                .OrderByDescending(u => u.Version)
                .FirstOrDefault(u => u.Version > _applicationInfo.Version);

            if (update != null)
            {
                var isSkipped = await _updateStorage.CheckSkipUpdate(update.Version);
                if (!isSkipped)
                {
                    return update;
                }
            }
            
            return null;
        }

        public async Task SkipUpdate(Version version)
        {
            await _updateStorage.SaveSkipUpdate(version);
        }
    }
}