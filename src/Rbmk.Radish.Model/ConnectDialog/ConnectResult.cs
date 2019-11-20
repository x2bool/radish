using System;
using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.ConnectDialog
{
    public abstract class ConnectResult
    {
        public class Created : ConnectResult
        {
            public RedisConnectionInfo ConnectionInfo { get; }

            public Created(RedisConnectionInfo connectionInfo)
            {
                ConnectionInfo = connectionInfo;
            }
        }
        
        public class Failed : ConnectResult
        {
            public Exception Exception { get; }

            public Failed(Exception exception)
            {
                Exception = exception;
            }
        }
        
        public class Cancelled : ConnectResult
        {
        }
    }
}