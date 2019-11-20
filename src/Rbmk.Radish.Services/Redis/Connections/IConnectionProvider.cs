using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rbmk.Radish.Services.Redis.Connections
{
    public interface IConnectionProvider
    {
        IObservable<RedisConnectionInfo> Restore();
        
        IObservable<RedisConnectionInfo> Connect(
            string connectionName,
            string connectionString);

        IObservable<RedisConnectionInfo> Reconnect(
            RedisConnectionInfo oldConnectionInfo,
            string newConnectionName = null,
            string newConnectionString = null);

        IObservable<RedisConnectionInfo> Close(
            RedisConnectionInfo oldConnectionInfo);
        
        IObservable<RedisConnectionInfo> Disconnect(
            RedisConnectionInfo oldConnectionInfo);

        IObservable<RedisConnectionInfo> GetConnections();

        IObservable<RedisConnectionInfo> GetConnection(Guid id);
    }
}