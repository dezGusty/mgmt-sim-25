using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Utils
{
    public static class PasswordGenerator
    {
        public static string GenerateSimpleCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            using (var rng = new RNGCryptoServiceProvider())
            {
                var code = new StringBuilder();
                var buffer = new byte[4];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    uint randomValue = BitConverter.ToUInt32(buffer, 0);
                    code.Append(chars[(int)(randomValue % chars.Length)]);
                }

                return code.ToString();
            }
        }
    }
}
