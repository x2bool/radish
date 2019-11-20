using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rbmk.Radish.Services.Persistence;
using Rbmk.Radish.Services.Persistence.Entities;

namespace Rbmk.Radish.Services.Redis.Connections
{
    public class ConnectionStorage : IConnectionStorage
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;

        public ConnectionStorage(
            IDatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task AddAsync(ConnectionEntity connectionEntity)
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                using (var transaction = await db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var order = await db.Connections
                            .DefaultIfEmpty()
                            .MaxAsync(c => c.Order);
                        
                        connectionEntity.Order = order + 1;
                        
                        db.Connections.Add(connectionEntity);
                        await db.SaveChangesAsync();
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task UpdateAsync(ConnectionEntity connectionEntity)
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                using (var transaction = await db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var originalEntity = await db.Connections.FirstOrDefaultAsync(
                            c => c.Id == connectionEntity.Id);

                        if (originalEntity != null)
                        {
                            originalEntity.Name = connectionEntity.Name;
                            originalEntity.ConnectionString = connectionEntity.ConnectionString;

                            db.Connections.Update(originalEntity);
                            await db.SaveChangesAsync();
                        }
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                var connectionEntity = await db.Connections.FirstOrDefaultAsync(c => c.Id == id);
                if (connectionEntity != null)
                {
                    db.Connections.Remove(connectionEntity);
                }
                
                await db.SaveChangesAsync();
            }
        }

        public async Task<IList<ConnectionEntity>> GetAllAsync()
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                var connectionEntities = await db.Connections
                    .OrderBy(c => c.Order)
                    .ToListAsync();
                
                return connectionEntities;
            }
        }
    }
}