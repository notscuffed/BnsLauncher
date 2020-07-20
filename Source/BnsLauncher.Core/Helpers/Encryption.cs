using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BnsLauncher.Core.Helpers
{
    // Just so the passwords are not stored in plain text
    public static class Encryption
    {
        private const int Iterations = 1000;
        private static readonly string Password = GeneratePassword(Environment.MachineName + Environment.ProcessorCount + Environment.UserDomainName);

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                var saltBytes = Generate256BitEntropy();
                var ivBytes = Generate256BitEntropy();

                var inputBytes = Encoding.UTF8.GetBytes(plainText);

                var keyBytes = new Rfc2898DeriveBytes(Password, saltBytes, Iterations).GetBytes(32);

                using var rijandelManaged = new RijndaelManaged
                {
                    BlockSize = 256,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                };

                using var encryptor = rijandelManaged.CreateEncryptor(keyBytes, ivBytes);
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                cryptoStream.FlushFinalBlock();

                return Convert.ToBase64String(saltBytes
                    .Concat(ivBytes)
                    .Concat(memoryStream.ToArray()).ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                var saltAndIvBytes = Convert.FromBase64String(encryptedText);
                var saltBytes = saltAndIvBytes.Take(32).ToArray();
                var ivBytes = saltAndIvBytes.Skip(32).Take(32).ToArray();

                var encryptedBytes = saltAndIvBytes.Skip(64).Take(saltAndIvBytes.Length - 64).ToArray();

                var keyBytes = new Rfc2898DeriveBytes(Password, saltBytes, Iterations).GetBytes(32);

                using var symmetricKey = new RijndaelManaged
                {
                    BlockSize = 256,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };

                using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivBytes);
                using var memoryStream = new MemoryStream(encryptedBytes);
                using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                var decryptedBytes = new byte[encryptedBytes.Length];
                var read = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes, 0, read);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GeneratePassword(string input)
        {
            using var sha256 = SHA256.Create();

            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            return string.Concat(hashBytes.Select(x => $"{x:X2}"));
        }

        private static byte[] Generate256BitEntropy()
        {
            var bytes = new byte[32];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return bytes;
        }
    }
}