using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rbmk.Radish.Services.Persistence;
using Rbmk.Radish.Services.Persistence.Entities;

namespace Rbmk.Radish.Services.Licenses
{
    public class LicenseStorage : ILicenseStorage
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;

        public LicenseStorage(
            IDatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }
        
        public Task<LicenseEntity> GetAsync()
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                return db.Licenses.FirstOrDefaultAsync();
            }
        }

        public Task SetAsync(LicenseEntity license)
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                db.Licenses.Add(license);
                return db.SaveChangesAsync();
            }
        }

        public async Task ClearAsync()
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                var licenses = await db.Licenses.ToListAsync();
                db.Licenses.RemoveRange(licenses);
                await db.SaveChangesAsync();
            }
        }
    }
}