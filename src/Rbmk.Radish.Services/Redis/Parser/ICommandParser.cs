using System;

namespace Rbmk.Radish.Services.Redis.Parser
{
    public interface ICommandParser
    {
        IObservable<RedisBatchInfo> Parse(string commandText);
    }
}