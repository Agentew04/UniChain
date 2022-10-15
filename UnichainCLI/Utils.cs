using Newtonsoft.Json.Linq;
using System.Reflection;
using Unichain.Core;
using Unichain.Parsing;

namespace Unichain.CLI
{
    public static class Utils
    {
        public static string GetVersion() {
            var ver = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if(ver is null)
                throw new Exception("Could not get version");
            return ver;
        }

        public static List<T> Clone<T>(this List<T> obj) {
            var clone = new List<T>();
            foreach (var item in obj)
                clone.Add(item);
            return clone;
        }

        public static string SanitizePath(string filePath, string defaultName, string ext) {
            FileAttributes? attr;
            if(Directory.Exists(filePath) || File.Exists(filePath))
                attr = File.GetAttributes(filePath);
            else
                attr = null;

            // is directory
            if (attr is not null && (attr?.HasFlag(FileAttributes.Directory) ?? false)) {
                filePath = Path.Combine(filePath, $"\\{defaultName}{ext}");
            }

            if (Path.GetExtension(filePath) != ext) {
                filePath = Path.ChangeExtension(filePath, ext);
            }

            return filePath;
        }

        public static void Print(string s) => Console.WriteLine(s);


        public static bool TryGetChainPath(string[] args, out string filepath)
        {
            filepath = "";
            int flagindex = 0;
            if (args == null || args.Length == 0) return false;
            if (!(args.Contains("-f") || args.Contains("--file"))) return false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-f" || args[i] == "--file")
                {
                    flagindex = i;
                    break;
                }
            }
            try
            {
                filepath = args[flagindex + 1];
            }
            catch (Exception)
            {
                return false;
            }
            if (!File.Exists(filepath)) return false;
            filepath = Path.GetFullPath(filepath);
            if (Path.GetExtension(filepath) != ".chain") return false;
            return true;
        }

        public static void CreateChain(string path)
        {
            Blockchain blockchain = new();
            SaveBlockChain(path, blockchain);
        }

        public static void SaveBlockChain(string path, Blockchain blockchain)
        {
            BlockchainParser parser = new();
            using MemoryStream ms = parser.SerializeBlockchain(blockchain);
            try
            {
                File.WriteAllBytes(path, ms.ToArray());
            }
            catch (Exception ex)
            {
                Utils.Print(ex.Message);
                Environment.Exit(2);
                return;
            }
        }

        public static Blockchain? ParseBlockchain(string path)
        {
            using BlockchainParser parser = new();
            using FileStream stream = new(path, FileMode.Open);
            Blockchain blockchain = parser.DeserializeBlockchain(stream);
            if (blockchain == null)
            {
                Print("Failed to load blockchain!");
                return null;
            }
            return blockchain;
        }
    }
}
