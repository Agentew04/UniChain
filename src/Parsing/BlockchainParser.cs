using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unichain.Core;
using Unichain.Events;

namespace Unichain.Parsing
{
    public class BlockchainParser : IDisposable
    {
        private readonly List<IDisposable> streams = new();

        /// <summary>
        /// Serializes a blockchain with AES encryption(2048 key and block size)
        /// </summary>
        /// <param name="blockchain">The blockchain that will be serialized</param>
        /// <param name="auth">A object containing the Key and IV</param>
        /// <returns>The Stream containing all data</returns>
        public MemoryStream SerializeBlockchain(Blockchain blockchain, StreamEncryptor.Auth? auth)
        {
            bool isencrypted = !(auth is null || auth.Key is null || auth.IV is null);
            MemoryStream memoryStream = new();
            streams.Add(memoryStream);
            ZipFile zipfile = new();
            streams.Add(zipfile);
            AddBlocks(zipfile, blockchain.Chain);
            AddBlockchainInfo(zipfile, blockchain);
            zipfile.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            if (isencrypted)
                return StreamEncryptor.EncryptStream(memoryStream, auth!);
            else
                return memoryStream;

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
            streams.Add(stream);
            streams.Add(zipFile);
            var (diff, reward) = GetBlockchainInfo(zipFile);
            var blockchain = new Blockchain()
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
                var (sector, subindex) = GetSector(i);
                MemoryStream blockStream = SerializeBlock(chain[i]);
                entries.Add(zipfile.AddEntry($"chain\\{sector}\\{subindex}.block", blockStream));
                streams.Add(blockStream);
            }
            return entries;
        }

        private IEnumerable<Block> GetBlocks(ZipFile zipfile)
        {
            Regex isBlock = new(@"^chain\/[0-9|a-f]+\/\d+\.block$");
            Regex getSector = new(@"(?<=chain\/)[0-9|a-f]+(?=\/\d+\.block$)");
            Regex getNum = new(@"(?<=\/)\d+?(?=\.block$)");
            var blocks = zipfile.Where(entry => isBlock.IsMatch(entry.FileName));
            Queue<ZipEntry> queue = new(blocks);
            while (queue.Count > 0)
            {
                MemoryStream ms = new();
                streams.Add(ms); //was using; here
                // must extract to memory
                queue.Dequeue().Extract(ms);
                yield return DeserializeBlock(ms);
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
            BinaryWriter binaryWriter = new(memoryStream);
            binaryWriter.Write(blockchain.Difficulty);
            binaryWriter.Write(blockchain.Reward);
            binaryWriter.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            var entry = zipfile.AddEntry("info.bin", memoryStream);
            streams.Add(memoryStream);
            streams.Add(binaryWriter);
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
            using BinaryWriter binaryWriter = new(infoMStream); // i think these can stay disposed here
            binaryWriter.Write(block.Index);
            binaryWriter.Write(block.Timestamp);
            binaryWriter.Write(block.Nonce);
            binaryWriter.Write(block.PreviousHash ?? "");
            binaryWriter.Write(block.Hash);
            infoMStream.Seek(0, SeekOrigin.Begin);

            ZipFile blockfile = new();
            blockfile.AddEntry("info.bin", infoMStream);

            List<string> events = new();
            if (block.Events != null)
                foreach (var @event in block.Events)
                    events.Add(GetJsonDataFromEvent(@event));
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
            // it's not recognizing as zipfile
            using ZipFile zipFile = ZipFile.Read(zipStream);
            using var binStream = zipFile.Where(x => x.FileName.Contains("info.bin")).FirstOrDefault()?.OpenReader();
            // todo add custom exception
            if(binStream is null)
                throw new Exception("Invalid block file!");
            using BinaryReader binaryReader = new(binStream);
            var index = binaryReader.ReadInt32();
            var timestamp = binaryReader.ReadInt64();
            var nonce = binaryReader.ReadInt32();
            var previousHash = binaryReader.ReadString();
            var hash = binaryReader.ReadString();

            using var eventsStream = zipFile.Where(x => x.FileName.Contains("events.json")).FirstOrDefault()?.OpenReader();
            if(eventsStream is null)
                throw new Exception("events.json not found");
            using StreamReader streamReader = new(eventsStream);
            var transactions = JsonConvert.DeserializeObject<List<string>>(streamReader.ReadToEnd()) ?? new();
            List<ITransaction> events = new();
            foreach (var transaction in transactions)
            {
                var deserialized = JsonConvert.DeserializeObject<ITransaction>(transaction);
                if (deserialized is not null)
                    events.Add(deserialized);
                
                // because of the compression, plaintext is the best way to go
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
        /// Encodes the transaction in JSON and put it in a string
        /// </summary>
        /// <param name="e">The event to be converted</param>
        /// <returns>A base64 string</returns>
        private static string GetJsonDataFromEvent(ITransaction e)
        {
            var json = JsonConvert.SerializeObject(e);
            return json;
            // plain text is better than encoded because of the compression
            // or idk, i tested and this was better than the others
            //byte[] bytes = Encoding.UTF8.GetBytes(json);
            //return Convert.ToBase64String(bytes);
            //return Base85.Z85.Encode(bytes);
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
            int subindex = (int)((((decimal)currentIndex / 100m) - (decimal)sectorInt) * 100m);
            string sectorHex = sectorInt.ToString("X");
            return (sectorHex, subindex);
        }

        public void Dispose()
        {
            foreach (var disposable in streams)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception)
                {
                    continue;
                }
            }
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
