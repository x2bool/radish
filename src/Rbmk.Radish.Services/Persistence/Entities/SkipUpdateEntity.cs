using System;

namespace Rbmk.Radish.Services.Persistence.Entities
{
    public class SkipUpdateEntity
    {
        public Guid Id { get; set; }
        
        public DateTimeOffset DateTime { get; set; }
        
        public string Version { get; set; }
    }
}