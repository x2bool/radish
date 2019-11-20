using System;
using Rbmk.Radish.Services.Redis.Executor;

namespace Rbmk.Radish.Services.Redis
{
    public class RedisDatabaseInfo : RedisTargetInfo
    {
        public int Number { get; }

        public RedisDatabaseInfo(
            Guid id,
            RedisConnectionInfo connectionInfo,
            int number)
            : base(id, connectionInfo)
        {
            Number = number;
        }

        public RedisDatabaseInfo(
            RedisConnectionInfo connectionInfo,
            int number)
            : this(
                Guid.NewGuid(),
                connectionInfo,
                number)
        {
        }
    }
}