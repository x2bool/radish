using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rbmk.Radish.Services.Persistence.Entities;

namespace Rbmk.Radish.Services.Redis.Connections
{
    public interface IConnectionStorage
    {
        Task AddAsync(ConnectionEntity connectionEntity);

        Task UpdateAsync(ConnectionEntity connectionEntity);

        Task RemoveAsync(Guid id);

        Task<IList<ConnectionEntity>> GetAllAsync();
    }
}