using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CSRedis;
using Rbmk.Radish.Services.Broadcasts.Connections;
using Rbmk.Radish.Services.Persistence.Entities;
using Rbmk.Utils.Broadcasts;

namespace Rbmk.Radish.Services.Redis.Connections
{
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly IBroadcastService _broadcastService;
        private readonly IConnectionStorage _connectionStorage;
        private readonly IClientAccessor _clientAccessor;

        private readonly List<RedisConnectionInfo> _connectionRegistry
            = new List<RedisConnectionInfo>();

        public ConnectionProvider(
            IBroadcastService broadcastService,
            IConnectionStorage connectionStorage,
            IClientAccessor clientAccessor)
        {
            _broadcastService = broadcastService;
            _connectionStorage = connectionStorage;
            _clientAccessor = clientAccessor;
        }

        public IObservable<RedisConnectionInfo> Restore()
        {
            return _connectionStorage.GetAllAsync()
                .ToObservable()
                .SelectMany(list => list)
                .Select(connectionEntity => GetConnection(
                    connectionEntity.Name,
                    connectionEntity.ConnectionString,
                    connectionEntity.Id))
                .Do(connectionInfo =>
                {
                    _connectionRegistry.Add(connectionInfo);
                })
                .Do(connectionInfo =>
                {
                    _broadcastService.Broadcast(
                        new ConnectionBroadcast(connectionInfo.Id, ConnectionBroadcastKind.Restore));
                });
        }
        
        public IObservable<RedisConnectionInfo> Connect(
            string connectionName,
            string connectionString)
        {
            return Observable.Return(GetConnection(connectionName, connectionString))
                .SelectMany(connectionInfo =>
                {
                    connectionInfo.ServerInfos.AddRange(GetServers(connectionInfo));
                    
                    return connectionInfo.ServerInfos
                        .ToObservable()
                        .SelectMany(s =>
                        {
                            return _clientAccessor.Init(s)
                                .Catch<RedisClient, Exception>(
                                    e => Observable.Return(default(RedisClient)));
                        })
                        .ToList()
                        .Select(clients =>
                        {
                            if (clients.Any(c => c.IsConnected))
                            {
                                connectionInfo.DatabaseInfos.AddRange(GetDatabases(connectionInfo));
                            }
                            else
                            {
                                connectionInfo.ServerInfos.Clear();
                            }
                            
                            return connectionInfo;
                        });
                })
                .Do(async connectionInfo =>
                {
                    _connectionRegistry.Add(connectionInfo);
                    await _connectionStorage.AddAsync(new ConnectionEntity
                    {
                        Name = connectionName,
                        ConnectionString = connectionString
                    });
                    _broadcastService.Broadcast(
                        new ConnectionBroadcast(connectionInfo.Id, ConnectionBroadcastKind.Connect));
                });
        }

        public IObservable<RedisConnectionInfo> Reconnect(
            RedisConnectionInfo oldConnectionInfo,
            string newConnectionName = null,
            string newConnectionString = null)
        {   
            return Observable.Return(GetConnection(
                newConnectionName ?? oldConnectionInfo.Name,
                newConnectionString ?? oldConnectionInfo.ConnectionString,
                oldConnectionInfo.Id))
                .SelectMany(connectionInfo =>
                {
                    connectionInfo.ServerInfos.AddRange(GetServers(connectionInfo, oldConnectionInfo));
                    
                    return connectionInfo.ServerInfos
                        .ToObservable()
                        .SelectMany(s =>
                        {
                            return _clientAccessor.Init(s)
                                .Catch<RedisClient, Exception>(
                                    e => Observable.Return(default(RedisClient)));
                        })
                        .ToList()
                        .Select(clients =>
                        {
                            if (clients.Any(c => c.IsConnected))
                            {
                                connectionInfo.DatabaseInfos.AddRange(GetDatabases(connectionInfo, oldConnectionInfo));
                            }
                            else
                            {
                                connectionInfo.ServerInfos.Clear();
                            }

                            return connectionInfo;
                        });
                })
                .Do(async connectionInfo =>
                {
                    int i = _connectionRegistry.IndexOf(oldConnectionInfo);
                    _connectionRegistry[i] = connectionInfo;
                    await _connectionStorage.UpdateAsync(new ConnectionEntity
                    {
                        Id = oldConnectionInfo.Id,
                        Name = newConnectionName ?? oldConnectionInfo.Name,
                        ConnectionString = newConnectionString ?? oldConnectionInfo.ConnectionString
                    });
                    _broadcastService.Broadcast(
                        new ConnectionBroadcast(connectionInfo.Id, ConnectionBroadcastKind.Reconnect));
                });
        }

        public IObservable<RedisConnectionInfo> Close(
            RedisConnectionInfo oldConnectionInfo)
        {   
            return oldConnectionInfo.ServerInfos
                .ToObservable()
                .ToList()
                .SelectMany(serverInfos =>
                {
                    return serverInfos
                        .ToObservable()
                        .SelectMany(s => _clientAccessor.Release(s))
                        .ToList();
                })
                .Do(async _ =>
                {
                    _connectionRegistry.Remove(oldConnectionInfo);
                    await _connectionStorage.RemoveAsync(oldConnectionInfo.Id);
                    _broadcastService.Broadcast(
                        new ConnectionBroadcast(oldConnectionInfo.Id, ConnectionBroadcastKind.Close));
                })
                .Select(_ => oldConnectionInfo);
        }

        public IObservable<RedisConnectionInfo> Disconnect(
            RedisConnectionInfo oldConnectionInfo)
        {
            return oldConnectionInfo.ServerInfos
                .ToObservable()
                .ToList()
                .SelectMany(serverInfos =>
                {
                    return serverInfos
                        .ToObservable()
                        .SelectMany(s => _clientAccessor.Release(s))
                        .ToList();
                })
                .Select(_ =>
                {
                    _broadcastService.Broadcast(
                        new ConnectionBroadcast(oldConnectionInfo.Id, ConnectionBroadcastKind.Disconnect));
                    
                    return oldConnectionInfo;
                });
        }

        public IObservable<RedisConnectionInfo> GetConnections()
        {
            return _connectionRegistry.ToList()
                .ToObservable();
        }

        public IObservable<RedisConnectionInfo> GetConnection(Guid id)
        {
            return _connectionRegistry.ToList()
                .ToObservable()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        private RedisConnectionInfo GetConnection(
            string connectionName,
            string connectionString,
            Guid? id = null)
        {
            var connectionInfo = id.HasValue
                ? new RedisConnectionInfo(id.Value, connectionName, connectionString)
                : new RedisConnectionInfo(connectionName, connectionString);

            return connectionInfo;
        }

        private IEnumerable<RedisServerInfo> GetServers(
            RedisConnectionInfo connectionInfo, RedisConnectionInfo oldConnectionInfo = null)
        {
            if (oldConnectionInfo == null)
            {
                oldConnectionInfo = connectionInfo;
            }
            
            var targets = connectionInfo.ConnectionString.Split(';', ',');
            
            for (int i = 0; i < targets.Length; i++)
            {
                var parts = targets[i].Split(':');
                var host = parts[0];
                var port = int.Parse(parts[1]);
                
                var endpoint = new DnsEndPoint(host, port);
                var server = oldConnectionInfo.ServerInfos.FirstOrDefault(
                    s => s.EndPoint.Host == endpoint.Host && s.EndPoint.Port == endpoint.Port);

                if (server != null)
                {
                    yield return new RedisServerInfo(server.Id, connectionInfo, endpoint);
                }
                else
                {
                    yield return new RedisServerInfo(connectionInfo, endpoint);
                }
            }
        }

        private IEnumerable<RedisDatabaseInfo> GetDatabases(
            RedisConnectionInfo connectionInfo, RedisConnectionInfo oldConnectionInfo = null)
        {
            if (oldConnectionInfo == null)
            {
                oldConnectionInfo = connectionInfo;
            }
            
            for (int i = 0; i < 16; i++)
            {
                if (i < oldConnectionInfo.DatabaseInfos.Count)
                {
                    yield return new RedisDatabaseInfo(
                        oldConnectionInfo.DatabaseInfos[i].Id, connectionInfo, i);
                }
                else
                {
                    yield return new RedisDatabaseInfo(
                        connectionInfo, i);
                }
            }
        }
    }
}