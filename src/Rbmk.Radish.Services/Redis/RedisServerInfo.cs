using System;
using System.Net;
using CSRedis;
using Rbmk.Radish.Services.Redis.Executor;

namespace Rbmk.Radish.Services.Redis
{
    public class RedisServerInfo : RedisTargetInfo
    {   
        public DnsEndPoint EndPoint { get; set; }

        public RedisServerInfo(
            Guid id,
            RedisConnectionInfo connectionInfo,
            DnsEndPoint endPoint)
            : base(id, connectionInfo)
        {
            EndPoint = endPoint;
        }

        public RedisServerInfo(
            RedisConnectionInfo connectionInfo,
            DnsEndPoint endPoint)
            : this (
                Guid.NewGuid(),
                connectionInfo,
                endPoint)
        {
        }
    }
}