using System;

namespace Rbmk.Radish.Services.Persistence.Entities
{
    public class LicenseEntity
    {
        public Guid Id { get; set; }
        
        public string Base64 { get; set; }
    }
}