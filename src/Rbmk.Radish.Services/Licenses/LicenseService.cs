using System;
using System.Threading.Tasks;
using Rbmk.Radish.Services.Persistence.Entities;
using Rbmk.Utils.Licenses;

namespace Rbmk.Radish.Services.Licenses
{
    public class LicenseService : ILicenseService
    {
        private readonly ILicenseStorage _licenseStorage;
        private readonly ILicenseChecker _licenseChecker;

        public LicenseService(
            ILicenseStorage licenseStorage,
            ILicenseChecker licenseChecker)
        {
            _licenseStorage = licenseStorage;
            _licenseChecker = licenseChecker;
        }
        
        public async Task<bool> CheckCurrentLicenseAsync()
        {
            var licenseEntity = await _licenseStorage.GetAsync();
            if (licenseEntity == null)
            {
                return false;
            }

            License license;
            try
            {
                license = License.FromBase64(licenseEntity.Base64);
            }
            catch
            {
                license = null;
            }

            if (license == null)
            {
                return false;
            }

            if (DateTimeOffset.UtcNow > license.ValidUntilDate)
            {
                return false;
            }

            return license.Type == LicenseType.Trial
                || _licenseChecker.CheckSignature(license);
        }

        public async Task<License> GetCurrentLicenseAsync()
        {
            var license = await _licenseStorage.GetAsync();
            if (license == null)
            {
                return null;
            }
            
            return License.FromBase64(license.Base64);
        }

        public async Task SetCurrentLicenseAsync(License license)
        {
            await _licenseStorage.ClearAsync();
            await _licenseStorage.SetAsync(new LicenseEntity
            {
                Id = Guid.Parse(license.Id),
                Base64 = License.ToBase64(license)
            });
        }
    }
}