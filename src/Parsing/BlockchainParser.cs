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

        public static MemoryStream SerializeBlockchain(Blockchain blockchain)
        {
            return SerializeBlockchain(blockchain, null);
        }

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

        private static ZipEntry AddBlockchainInfo(ZipFile zipfile, Blockchain blockchain)
        {
            using MemoryStream memoryStream = new();
            using BinaryWriter binaryWriter = new(memoryStream);

            binaryWriter.Write(blockchain.Difficulty);
            binaryWriter.Write(blockchain.Reward);

            return zipfile.AddEntry("info.bin", memoryStream);
        }

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

        private static string GetBase64DataFromEvent(BaseBlockChainEvent e)
        {
            var json = e.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }


        /// <summary>
        /// Returns a hexadecimal sector name for this block
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
