using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zip;
using Newtonsoft.Json;
using Unichain.Core;

namespace Unichain.Parsing
{
    public class BlockchainParser
    {
        /// <summary>
        /// Serializes a blockchain with AES encryption(2048 key and block size)
        /// </summary>
        /// <param name="blockchain">The blockchain that will be serialized</param>
        /// <param name="auth">A object containing the Key and IV</param>
        /// <returns>The Stream containing all data</returns>
        public static MemoryStream SerializeBlockchain(Blockchain blockchain, StreamEncryptor.Auth auth) 
        {
            bool isencrypted = auth == null || auth.Key==null || auth.Key == Array.Empty<byte>();
            using ZipFile zipfile = new();
            AddBlocks(zipfile, blockchain.Chain);
            AddBlockchainInfo(zipfile, blockchain);
            using MemoryStream memoryStream = new();
            zipfile.Save(memoryStream);

            if (isencrypted)
            {
                return StreamEncryptor.EncryptStream(memoryStream, auth);
            }
            else return memoryStream;
        }

        /// <summary>
        /// Serializes a blockchain without any encryption
        /// </summary>
        /// <param name="blockchain">The blockchain to be serialized</param>
        /// <returns>The Stream containg all the data</returns>
        public static MemoryStream SerializeBlockchain(Blockchain blockchain)
        {
            return SerializeBlockchain(blockchain, null);
        }

        /// <summary>
        /// Adds all blocks from the chain to the <see cref="ZipFile"/>
        /// </summary>
        /// <param name="zipfile">The main <see cref="ZipFile"/></param>
        /// <param name="chain">The blockchain that will be used</param>
        /// <returns>A collection containing all entries created</returns>
        private static IEnumerable<ZipEntry> AddBlocks(ZipFile zipfile, List<Block> chain)
        {
            for (int i = 0; i < chain.Count; i++)
            {
                //get sector for this block
                var(sector,subindex) = GetSector(i);
                MemoryStream blockStream = CreateBlockFile(chain[i]);
                yield return zipfile.AddEntry($"chain\\{sector}\\{subindex}.block", blockStream);
            }
        }

        /// <summary>
        /// Adds to the main file a info.bin containing useful information to the blockchain
        /// </summary>
        /// <param name="zipfile">The main ZipFile</param>
        /// <param name="blockchain">The blockchain</param>
        /// <returns>The ZipEntry created</returns>
        private static ZipEntry AddBlockchainInfo(ZipFile zipfile, Blockchain blockchain)
        {
            using MemoryStream memoryStream = new();
            using BinaryWriter binaryWriter = new(memoryStream);

            binaryWriter.Write(blockchain.Difficulty);
            binaryWriter.Write(blockchain.Reward);

            return zipfile.AddEntry("info.bin", memoryStream);
        }

        /// <summary>
        /// Creates a <see cref="ZipFile"/> containing the block and saves it to a memoryStream
        /// </summary>
        /// <param name="block">The block to be converted</param>
        /// <returns>The stream containing the zip bytes</returns>
        private static MemoryStream CreateBlockFile(Block block)
        {
            using MemoryStream blockStream = new();
            using MemoryStream infoMStream = new();
            using BinaryWriter binaryWriter = new(infoMStream);
            binaryWriter.Write(block.Index);
            binaryWriter.Write(block.Timestamp);
            binaryWriter.Write(block.PreviousHash);
            binaryWriter.Write(block.Hash);
            binaryWriter.Flush();

            using ZipFile blockfile = new();
            blockfile.AddEntry("info.bin", infoMStream);
            
            List<string> events = new();
            foreach(var @event in block.Events) events.Add(GetBase64DataFromEvent(@event));
            var eventsjson = JsonConvert.SerializeObject(events);
            blockfile.AddEntry("events.json", eventsjson);
            blockfile.Save(blockStream);
            return blockStream;
        }

        /// <summary>
        /// Encodes the JSON bytes in Base 64 and put it in a string
        /// </summary>
        /// <param name="e">The event to be converted</param>
        /// <returns>A base64 string</returns>
        private static string GetBase64DataFromEvent(BaseBlockChainEvent e)
        {
            var json = e.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }


        /// <summary>
        /// Returns a hexadecimal sector name for this block and its sub-index
        /// </summary>
        /// <param name="totalLength"></param>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        private static (string sector, int subindex) GetSector(int currentIndex)
        {
            // 100 blocks per sector
            var sectorDecimalNumber = Math.Floor((decimal)currentIndex / 100);
            var subindex = (int)((currentIndex / 100) - sectorDecimalNumber) * 100;
            return (sectorDecimalNumber.ToString("X"), subindex);
        }
    }
}
