using System;

namespace Rbmk.Radish.Services.Persistence.Entities
{
    public class ConnectionEntity
    {
        public Guid Id { get; set; }
        
        public int Order { get; set; }
        
        public string Name { get; set; }
        
        public string ConnectionString { get; set; }
    }
}