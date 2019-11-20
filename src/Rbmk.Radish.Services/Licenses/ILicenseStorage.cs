using System.Threading.Tasks;
using Rbmk.Radish.Services.Persistence.Entities;

namespace Rbmk.Radish.Services.Licenses
{
    public interface ILicenseStorage
    {
        Task<LicenseEntity> GetAsync();

        Task SetAsync(LicenseEntity license);

        Task ClearAsync();
    }
}