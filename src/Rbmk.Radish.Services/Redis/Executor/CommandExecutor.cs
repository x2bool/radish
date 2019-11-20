using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CSRedis;
using Rbmk.Radish.Services.Redis.Connections;

namespace Rbmk.Radish.Services.Redis.Executor
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly IClientAccessor _clientAccessor;

        public CommandExecutor(
            IClientAccessor clientAccessor)
        {
            _clientAccessor = clientAccessor;
        }
        
        public IObservable<RedisResultInfo> Execute(
            RedisTargetInfo targetInfo, RedisCommandInfo commandInfo)
        {
            return _clientAccessor.With(targetInfo, client =>
                {
                    // TODO: async
                    return client.Call(commandInfo.Name, commandInfo.Args);
                })
                .Select(data => RedisResultInfo.MapResult(
                    targetInfo,
                    commandInfo,
                    data))
                .Catch<RedisResultInfo, RedisException>(exception => Observable.Return(
                    new RedisResultInfo.Error(
                        targetInfo,
                        commandInfo,
                        exception)));
        }

        public IObservable<RedisResultInfo> Execute(
            RedisTargetInfo targetInfo, RedisBatchInfo batchInfo)
        {
            return _clientAccessor.With(targetInfo, client =>
            {
                return Observable.Create<RedisResultInfo>(observer =>
                {
                    foreach (var commandInfo in batchInfo.CommandInfos)
                    {
                        try
                        {
                            // TODO: async
                            var data = client.Call(commandInfo.Name, commandInfo.Args);

                            if (commandInfo.IsExec && batchInfo.IsTransaction)
                            {
                                observer.OnNext(
                                    RedisResultInfo.MapResult(
                                        targetInfo, batchInfo, commandInfo, data));
                            }
                            else
                            {
                                observer.OnNext(
                                    RedisResultInfo.MapResult(
                                        targetInfo, commandInfo, data));
                            }
                        }
                        catch (RedisException e)
                        {
                            observer.OnNext(new RedisResultInfo.Error(
                                targetInfo, commandInfo, e));
                        }
                    }
                
                    observer.OnCompleted();
                    return Disposable.Empty;
                });
            });
        }
    }
}