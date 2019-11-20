using System;

namespace Rbmk.Radish.Services.Redis
{
    public class RedisCommandInfo
    {   
        public Guid Id { get; }
        
        public string Name { get; }
        
        public string[] Args { get; }

        public RedisCommandInfo(
            Guid id,
            string name,
            params string[] args)
        {
            Id = id;
            Name = name;
            Args = args;
        }

        public RedisCommandInfo(
            string name,
            params string[] args)
            : this(
                Guid.NewGuid(),
                name,
                args)
        {
        }

        public bool IsMulti
            => Name.ToUpperInvariant() == "MULTI";

        public bool IsExec
            => Name.ToUpperInvariant() == "EXEC";

        public bool IsDiscard
            => Name.ToUpperInvariant() == "DISCARD";

        public bool IsWatch
            => Name.ToUpperInvariant() == "WATCH";

        public bool IsUnwatch
            => Name.ToUpperInvariant() == "UNWATCH";
    }
}