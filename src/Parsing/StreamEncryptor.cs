using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.Parsing
{
    public static class StreamEncryptor
    {
        public record Auth(byte[] Key, byte[] IV);

        public static MemoryStream EncryptStream(Stream inStream, Auth auth)
        {
            using Aes aes = Aes.Create();
            aes.Key = auth.Key ?? aes.Key;
            aes.IV = auth.IV ?? aes.IV;
            aes.KeySize = 2048;
            aes.BlockSize = 2048;
            aes.Padding = PaddingMode.PKCS7;
            ICryptoTransform encryptor = aes.CreateEncryptor();
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using StreamWriter swEncrypt = new(cryptoStream);
            swEncrypt.Write(inStream);
            return memoryStream;
        }

        public static MemoryStream DecryptStream(Stream inStream, Auth auth)
        {
            using Aes aes = Aes.Create();
            aes.Key = auth.Key ?? aes.Key;
            aes.IV = auth.IV ?? aes.IV;
            aes.KeySize = 2048;
            aes.BlockSize = 2048;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor();
            using CryptoStream cryptoStream = new(inStream, decryptor, CryptoStreamMode.Read);
            using MemoryStream memoryStream = new();
            CopyStream(cryptoStream, memoryStream);
            return memoryStream;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }

        }
    }
}
