using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace AlarmWorkflow.Tools.AutoUpdater.Models
{
    class HashHelper
    {
        internal static string GetHashOfString(string value)
        {
            HashAlgorithm alg = MD5.Create();
            using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(value)))
            {
                stream.Position = 0L;
                byte[] buffer = alg.ComputeHash(stream);

                return Convert.ToBase64String(buffer);
            }
        }
    }
}
