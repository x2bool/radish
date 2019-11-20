using System;
using System.Text;
using Newtonsoft.Json;

namespace Rbmk.Utils.Licenses
{
    public class License
    {
        public string Id { get; set; }
        
        public LicenseType Type { get; set; }
        
        public string FullName { get; set; }
        
        public string Company { get; set; }
        
        public DateTimeOffset IssuedAtDate { get; set; }
        
        public DateTimeOffset ValidUntilDate { get; set; }
        
        public string[] Signatures { get; set; }

        public static License FromBase64(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<License>(json);
        }

        public static string ToBase64(License license)
        {
            var json = JsonConvert.SerializeObject(license);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        public static License Trial =>
            new License
            {
                Id = Guid.Empty.ToString("N"),
                Type = LicenseType.Trial,
                IssuedAtDate = DateTimeOffset.UtcNow,
                ValidUntilDate = DateTimeOffset.UtcNow + TimeSpan.FromDays(31)
            };
    }
}