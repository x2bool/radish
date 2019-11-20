using System;

namespace Rbmk.Radish.Services.Redis
{
    public class RedisTargetInfo
    {
        public Guid Id { get; }
        
        public RedisConnectionInfo ConnectionInfo { get; }

        public RedisTargetInfo(
            Guid id,
            RedisConnectionInfo connectionInfo)
        {
            Id = id;
            ConnectionInfo = connectionInfo;
        }
    }
}