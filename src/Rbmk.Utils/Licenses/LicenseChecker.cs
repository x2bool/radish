using System;
using System.Security.Cryptography;
using System.Text;

namespace Rbmk.Utils.Licenses
{
    public class LicenseChecker : ILicenseChecker
    {
        public bool CheckSignature(License license)
        {
            var signature = license.Signatures[0];

            return F(0, license.Id) == signature[0]
                && F(1, license.Id) == signature[1];
        }

        public string GenerateSignature(License license)
        {
            return new string(
                new [] {
                    F(0, license.Id),
                    F(1, license.Id)
                });
        }

        private static char F(int i, object data)
        {
            string[] magic =
            {
                "b2ff021583664f709fb12eda95ae0377",
                "1c3012cfdaa94a7b934ad11712c0b35c"
            };

            var ch = Sha256(magic[i] + data)[i];
            return ch;
        }
        
        private static string Sha256(string value)
        {
            var sb = new StringBuilder();

            using (var hash = SHA256.Create())            
            {
                var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
            }

            return sb.ToString();
        }
    }
}