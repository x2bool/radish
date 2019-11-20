using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rbmk.Radish.Services.Persistence;
using Rbmk.Radish.Services.Persistence.Entities;
using Version = Rbmk.Utils.Meta.Version;

namespace Rbmk.Radish.Services.Updates
{
    public class UpdateStorage : IUpdateStorage
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;

        public UpdateStorage(
            IDatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }
        
        public async Task SaveSkipUpdate(Version version)
        {
            var alreadySkipped = await CheckSkipUpdate(version);
            if (alreadySkipped)
            {
                return;
            }
            
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                db.SkipUpdates.Add(new SkipUpdateEntity
                {
                    Id = Guid.NewGuid(),
                    DateTime = DateTimeOffset.UtcNow,
                    Version = version.ToString()
                });

                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> CheckSkipUpdate(Version version)
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                var skipUpdate = await db.SkipUpdates
                    .FirstOrDefaultAsync(s => s.Version == version.ToString());

                return skipUpdate != null;
            }
        }
    }
}