using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zip;
using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using Unichain.Core;
using System.Text.RegularExpressions;

namespace Unichain.Parsing
{
    public class BlockchainParser
    {
        private readonly List<Stream> streams = new();
        private readonly MemoryStream outputStream = new();

        /// <summary>
        /// Serializes a blockchain with AES encryption(2048 key and block size)
        /// </summary>
        /// <param name="blockchain">The blockchain that will be serialized</param>
        /// <param name="auth">A object containing the Key and IV</param>
        /// <returns>The Stream containing all data</returns>
        public MemoryStream SerializeBlockchain(Blockchain blockchain, StreamEncryptor.Auth auth)
        {
            bool isencrypted = !(auth == null || auth.Key == null || auth.Key == Array.Empty<byte>());
            MemoryStream memoryStream = new();
            ZipFile zipfile = new();
            AddBlocks(zipfile, blockchain.Chain);
            AddBlockchainInfo(zipfile, blockchain);
            zipfile.Save(memoryStream);
            if (isencrypted)
            {
                return StreamEncryptor.EncryptStream(memoryStream, auth);
            }
            else
            {
                return memoryStream;
            }

        }

        /// <summary>
        /// Serializes a blockchain without any encryption
        /// </summary>
        /// <param name="blockchain">The blockchain to be serialized</param>
        /// <returns>The Stream containg all the data</returns>
        public MemoryStream SerializeBlockchain(Blockchain blockchain)
        {
            return SerializeBlockchain(blockchain, null);
        }

        public Blockchain DeserializeBlockchain(Stream stream)
        {
            ZipFile zipFile = ZipFile.Read(stream);
            var (diff,reward) = GetBlockchainInfo(zipFile);
            var blockchain = new Blockchain(1)
            {
                Chain = GetBlocks(zipFile).ToList(),
                Difficulty = diff,
                Reward = reward,
            };

            return blockchain;
        }


        #region Helpers

        /// <summary>
        /// Adds all blocks from the chain to the <see cref="ZipFile"/>
        /// </summary>
        /// <param name="zipfile">The main <see cref="ZipFile"/></param>
        /// <param name="chain">The blockchain that will be used</param>
        /// <returns>A collection containing all entries created</returns>
        private IEnumerable<ZipEntry> AddBlocks(ZipFile zipfile, List<Block> chain)
        {
            List<ZipEntry> entries = new();
            for (int i = 0; i < chain.Count; i++)
            {
                //get sector for this block
                var(sector,subindex) = GetSector(i);
                MemoryStream blockStream = SerializeBlock(chain[i]);
                streams.Add(blockStream);
                entries.Add(zipfile.AddEntry($"chain\\{sector}\\{subindex}.block", blockStream));
            }
            return entries;
        }

        private static IEnumerable<Block> GetBlocks(ZipFile zipfile)
        {
            var sectors = zipfile.Where(entry => entry.FileName.Contains("chain/"));
            PriorityQueue<ZipEntry,int> queue = new();
            foreach (var sector in sectors)
            {
                Regex regex = new Regex(@"\d+(?=.block)");
                var hex = regex.Match(sector.FileName).Value;
                var priority = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                queue.Enqueue(sector, priority);
            }
            while(queue.Count > 0)
            {
                var sector = queue.Dequeue();
                var reader = sector.OpenReader();
                yield return DeserializeBlock(reader);
            }
        }

        /// <summary>
        /// Adds to the main file a info.bin containing useful information to the blockchain
        /// </summary>
        /// <param name="zipfile">The main ZipFile</param>
        /// <param name="blockchain">The blockchain</param>
        /// <returns>The ZipEntry created</returns>
        private ZipEntry AddBlockchainInfo(ZipFile zipfile, Blockchain blockchain)
        {
            MemoryStream memoryStream = new();
            streams.Add(memoryStream);
            BinaryWriter binaryWriter = new(memoryStream);
            binaryWriter.Write(blockchain.Difficulty);
            binaryWriter.Write(blockchain.Reward);
            binaryWriter.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            var entry = zipfile.AddEntry("info.bin", memoryStream);
            return entry;
        }

        /// <summary>
        /// Gets the the main information from the info.bin located in the root folder
        /// </summary>
        /// <param name="zipFile">The ZipFile to be used</param>
        /// <returns>The difficulty and the reward</returns>
        private static (int difficulty, double reward) GetBlockchainInfo(ZipFile zipFile)
        {
            var reader = zipFile["info.bin"].OpenReader();
            using BinaryReader binaryReader = new(reader);
            var dif = binaryReader.ReadInt32();
            var reward = binaryReader.ReadDouble();
            return (dif, reward);
        }

        /// <summary>
        /// Creates a <see cref="ZipFile"/> containing the block and saves it to a memoryStream
        /// </summary>
        /// <param name="block">The block to be converted</param>
        /// <returns>The stream containing the zip bytes</returns>
        private static MemoryStream SerializeBlock(Block block)
        {
            MemoryStream blockStream = new();
            using MemoryStream infoMStream = new();
            using BinaryWriter binaryWriter = new(infoMStream);
            binaryWriter.Write(block.Index);
            binaryWriter.Write(block.Timestamp);
            binaryWriter.Write(block.Nonce);
            binaryWriter.Write(block.PreviousHash ?? "");
            binaryWriter.Write(block.Hash);
            infoMStream.Seek(0, SeekOrigin.Begin);

            ZipFile blockfile = new();
            blockfile.AddEntry("info.bin", infoMStream);
            
            List<string> events = new();
            if(block.Events!=null) 
                foreach (var @event in block.Events) 
                    events.Add(GetBase64DataFromEvent(@event));
            var eventsjson = JsonConvert.SerializeObject(events);
            blockfile.AddEntry("events.json", eventsjson);

            blockfile.Save(blockStream);
            blockStream.Seek(0, SeekOrigin.Begin);

            return blockStream;
        }

        /// <summary>
        /// Deserializes a block from a .block file
        /// </summary>
        /// <param name="zipStream"></param>
        /// <returns></returns>
        private static Block DeserializeBlock(Stream zipStream)
        {
            zipStream.Seek(0, SeekOrigin.Begin);
            using ZipFile zipFile = ZipFile.Read(zipStream);
            using var binStream = zipFile.Where(x => x.FileName.Contains("info.bin")).FirstOrDefault().OpenReader();
            using BinaryReader binaryReader = new(binStream);
            var index = binaryReader.ReadInt32();
            var timestamp = binaryReader.ReadInt64();
            var nonce = binaryReader.ReadInt32();
            var previousHash = binaryReader.ReadString();
            var hash = binaryReader.ReadString();

            using var eventsStream = zipFile.Where(x => x.FileName.Contains("events.json")).FirstOrDefault().OpenReader();
            using StreamReader streamReader = new(eventsStream);
            var eventdatalist = JsonConvert.DeserializeObject<List<string>>(streamReader.ReadToEnd());
            List<BaseBlockChainEvent> events = new();
            foreach (var eventdata in eventdatalist)
            {
                byte[] data = Convert.FromBase64String(eventdata);
                var json = Encoding.UTF8.GetString(data, 0, data.Length);
                events.Add(JsonConvert.DeserializeObject<BaseBlockChainEvent>(json));
            }
            return new Block()
            {
                Events = events,
                Hash = hash,
                Index = index,
                Timestamp = timestamp,
                PreviousHash = previousHash,
                Nonce = nonce,
            };
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
            int sectorInt = (int)Math.Floor((decimal)currentIndex / 100);
            int subindex = ((currentIndex / 100) - sectorInt) * 100;
            string sectorHex = sectorInt.ToString("X");
            return (sectorHex, subindex);
        }

        #endregion
    }
}
