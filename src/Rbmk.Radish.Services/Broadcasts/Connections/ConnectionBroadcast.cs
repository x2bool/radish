using System;
using Rbmk.Utils.Broadcasts;

namespace Rbmk.Radish.Services.Broadcasts.Connections
{
    public class ConnectionBroadcast : Broadcast
    {
        public Guid Id { get; }
        
        public ConnectionBroadcastKind Kind { get; }

        public ConnectionBroadcast(Guid id, ConnectionBroadcastKind kind)
        {
            Id = id;
            Kind = kind;
        }
    }
}