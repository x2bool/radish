using System;
using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public abstract class ConnectionNodeAction
    {
        public Guid Id { get; }
        
        public RedisConnectionInfo ConnectionInfo { get; }
        
        public ConnectionNodeAction(
            Guid id,
            RedisConnectionInfo connectionInfo)
        {
            Id = id;
            ConnectionInfo = connectionInfo;
        }
        
        public class Add : ConnectionNodeAction
        {
            public Add(Guid id, RedisConnectionInfo connectionInfo)
                : base(id, connectionInfo)
            {
            }
        }
        
        public class Delete : ConnectionNodeAction
        {
            public Delete(Guid id)
                : base(id, null)
            {
            }
        }
        
        public class Update : ConnectionNodeAction
        {   
            public Update(Guid id, RedisConnectionInfo connectionInfo)
                : base(id, connectionInfo)
            {
            }
        }
    }
}