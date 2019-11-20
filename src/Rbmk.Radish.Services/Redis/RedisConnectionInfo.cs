using System;
using System.Collections.Generic;
using CSRedis;

namespace Rbmk.Radish.Services.Redis
{
    public class RedisConnectionInfo
    {
        public Guid Id { get; }
        
        public string Name { get; }
        
        public string ConnectionString { get; }
        
        public List<RedisDatabaseInfo> DatabaseInfos { get; }
        
        public List<RedisServerInfo> ServerInfos { get; }

        public RedisConnectionInfo(
            Guid id,
            string name,
            string connectionString)
        {
            Id = id;
            Name = name;
            ConnectionString = connectionString;
            
            DatabaseInfos = new List<RedisDatabaseInfo>();
            ServerInfos = new List<RedisServerInfo>();
        }

        public RedisConnectionInfo(string name, string connectionString)
            : this(
                Guid.NewGuid(),
                name,
                connectionString)
        {
        }
    }
}