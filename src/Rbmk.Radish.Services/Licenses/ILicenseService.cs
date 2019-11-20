using System;
using System.Reactive;
using System.Threading.Tasks;
using Rbmk.Utils.Licenses;

namespace Rbmk.Radish.Services.Licenses
{
    public interface ILicenseService
    {
        Task<bool> CheckCurrentLicenseAsync();

        Task<License> GetCurrentLicenseAsync();
        
        Task SetCurrentLicenseAsync(License license);
    }
}