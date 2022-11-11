using System.IO;
using System.Security.Cryptography;

namespace Unichain.Parsing
{
    public static class StreamEncryptor
    {
        public record Auth(byte[] Key, byte[] IV);

        /// <summary>
        /// Encrypts a Stream using AES. Key and Block size is 2048 for safety.
        /// </summary>
        /// <param name="inStream">The stream that will be encrypted</param>
        /// <param name="auth">The Auth object</param>
        /// <returns>A stream containing all cypher data</returns>
        public static MemoryStream EncryptStream(Stream inStream, Auth auth)
        {
            using Aes aes = Aes.Create();
            aes.Key = auth?.Key ?? aes.Key;
            aes.IV = auth?.IV ?? aes.IV;
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

        /// <summary>
        /// Decrypts a <see cref="Stream"/> containing cyphertext. Uses AES with 2048 as Key and Block size.
        /// </summary>
        /// <param name="inStream">The encrypted <see cref="Stream"/></param>
        /// <param name="auth">A object containing Key and IV</param>
        /// <returns>A <see cref="Stream"/> containing decrypted data</returns>
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

        /// <summary>
        /// Copies a <see cref="Stream"/> to another in chunks
        /// </summary>
        /// <param name="input">The input</param>
        /// <param name="output">The output</param>
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
