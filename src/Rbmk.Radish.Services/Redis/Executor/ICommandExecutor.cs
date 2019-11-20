using System;

namespace Rbmk.Radish.Services.Redis.Executor
{
    public interface ICommandExecutor
    {
        IObservable<RedisResultInfo> Execute(
            RedisTargetInfo serverInfo, RedisCommandInfo commandInfo);

        IObservable<RedisResultInfo> Execute(
            RedisTargetInfo serverInfo, RedisBatchInfo batchInfo);
    }
}