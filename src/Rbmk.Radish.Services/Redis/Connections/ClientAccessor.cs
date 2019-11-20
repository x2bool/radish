using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using CSRedis;
using Rbmk.Utils.Reactive;

namespace Rbmk.Radish.Services.Redis.Connections
{
    public class ClientAccessor : IClientAccessor
    {
        // stores either client or exception
        private readonly ConcurrentDictionary<Guid, object> _clients
            = new ConcurrentDictionary<Guid, object>();
        
        private readonly SemaphoreSlim _semaphore
            = new SemaphoreSlim(1, 1);

        public IObservable<RedisClient> Init(
            RedisServerInfo serverInfo)
        {
            return Observable.Return(Unit.Default)
                .SelectMany(unit => _semaphore.WaitAsync()
                    .ToObservable()
                    .Select(_ => serverInfo))
                .Select(_ =>
                {
                    var obj = _clients.GetOrAdd(
                        serverInfo.Id,
                        id =>
                        {
                            try
                            {
                                return Connect(serverInfo.EndPoint);
                            }
                            catch (Exception exception)
                            {
                                return exception;
                            }
                        });

                    if (obj is Exception e)
                    {
                        throw e;
                    }

                    return (RedisClient) obj;
                })
                .Finally(() => _semaphore.Release());
        }

        public IObservable<RedisClient> Release(
            RedisServerInfo serverInfo)
        {
            return Observable.Return(Unit.Default)
                .SelectMany(unit => _semaphore.WaitAsync()
                    .ToObservable()
                    .Select(_ => serverInfo))
                .Select(_ =>
                {
                    if (_clients.TryRemove(serverInfo.Id, out var obj))
                    {
                        if (obj is Exception e)
                        {
                            throw e;
                        }
                        
                        var client = (RedisClient)obj;
                        client.Dispose();
                        return client;
                    }
                    
                    return null;
                })
                .Finally(() => _semaphore.Release());
        }

        public IObservable<RedisClient> Check(
            RedisServerInfo serverInfo)
        {
            return Observable.Return(Unit.Default)
                .SelectMany(unit => _semaphore.WaitAsync()
                    .ToObservable()
                    .Select(_ => serverInfo))
                .Select(_ =>
                {
                    if (_clients.TryGetValue(serverInfo.Id, out var obj))
                    {
                        if (obj is Exception e)
                        {
                            throw e;
                        }
                        
                        return (RedisClient)obj;
                    }
                    
                    return null;
                })
                .Finally(() => _semaphore.Release());
        }

        public IObservable<T> With<T>(
            RedisTargetInfo targetInfo,
            Func<RedisClient, Task<T>> func)
        {
            return GetServer(targetInfo)
                .SelectMany(serverInfo => _semaphore.WaitAsync()
                    .ToObservable()
                    .Select(_ => serverInfo))
                .Select(GetClient)
                .SelectSeq(client =>
                {
                    // TODO: async
                    if (targetInfo is RedisDatabaseInfo databaseInfo)
                    {
                        client.Select(databaseInfo.Number);
                    }
                    else
                    {
                        client.Select(0);
                    }
                    
                    // TODO: async
//                    return Task.Run(() => func(client))
//                        .ToObservable();
                    return func(client).ToObservable();
                })
                .Finally(() => _semaphore.Release());
        }

        public IObservable<T> With<T>(
            RedisTargetInfo targetInfo,
            Func<RedisClient, IObservable<T>> func)
        {
            return GetServer(targetInfo)
                .SelectMany(serverInfo => _semaphore.WaitAsync()
                    .ToObservable()
                    .Select(_ => serverInfo))
                .Select(GetClient)
                .SelectSeq(client =>
                {
                    // TODO: async
                    if (targetInfo is RedisDatabaseInfo databaseInfo)
                    {
                        client.Select(databaseInfo.Number);
                    }
                    else
                    {
                        client.Select(0);
                    }
                    
                    // TODO: async
//                    return Task.Run(() => func(client))
//                        .ToObservable()
//                        .SelectSeq(observables => observables);
                    return func(client);
                })
                .Finally(() => _semaphore.Release());
        }

        public IObservable<T> With<T>(
            RedisTargetInfo targetInfo,
            Func<RedisClient, T> func)
        {
            return GetServer(targetInfo)
                .SelectMany(serverInfo => _semaphore.WaitAsync()
                    .ToObservable()
                    .Select(_ => serverInfo))
                .Select(GetClient)
                .SelectSeq(client =>
                {
                    // TODO: async
                    if (targetInfo is RedisDatabaseInfo databaseInfo)
                    {
                        client.Select(databaseInfo.Number);
                    }
                    else
                    {
                        client.Select(0);
                    }
                    
                    // TODO: async
//                    return Task.Run(() => func(client))
//                        .ToObservable();
                    return Observable.Return(func(client));
                })
                .Finally(() => _semaphore.Release());
        }

        private RedisClient GetClient(
            RedisServerInfo serverInfo)
        {
            if (_clients.TryGetValue(serverInfo.Id, out var obj))
            {
                if (obj is Exception e)
                {
                    throw e;
                }
                        
                return (RedisClient)obj;
            }

            throw new Exception("No available client for the target");
        }
        
        private static IObservable<RedisServerInfo> GetServer(
            RedisTargetInfo targetInfo)
        {
            switch (targetInfo)
            {
                case RedisServerInfo serverInfo:
                    return Observable.Return(serverInfo);
                
                case RedisDatabaseInfo databaseInfo:
                    return Observable.Return(databaseInfo.ConnectionInfo.ServerInfos.First()); // TODO: negotiate
            }

            throw new NotSupportedException("Unknown target");
        }

        private static RedisClient Connect(
            DnsEndPoint endPoint)
        {
            var client = new RedisClient(endPoint);
            var isConnected = client.Connect(60 * 1000);

            if (!isConnected)
            {
                throw new Exception("Could not connect");
            }

            return client;
        }
    }
}