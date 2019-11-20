using System;
using System.Reactive;
using System.Threading.Tasks;
using CSRedis;

namespace Rbmk.Radish.Services.Redis.Connections
{
    public interface IClientAccessor
    {
        IObservable<RedisClient> Init(RedisServerInfo serverInfo);
        
        IObservable<RedisClient> Release(RedisServerInfo serverInfo);

        IObservable<RedisClient> Check(RedisServerInfo serverInfo);

        IObservable<T> With<T>(
            RedisTargetInfo targetInfo,
            Func<RedisClient, Task<T>> func);
        
        IObservable<T> With<T>(
            RedisTargetInfo targetInfo,
            Func<RedisClient, IObservable<T>> func);
        
        IObservable<T> With<T>(
            RedisTargetInfo targetInfo,
            Func<RedisClient, T> func);
    }
}