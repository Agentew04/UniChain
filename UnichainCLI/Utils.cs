using Unichain.Core;
using Unichain.Parsing;

namespace Unichain.CLI
{
    public static class Utils
    {

        public static bool HasFlag(string[] args, Flag flag)
        {
            if (args.Length == 0)
            {
                return false;
            }
            if (args == null)
            {
                return false;
            }
            var result = args.Contains(flag.Name) || args.Contains(flag.Simplified);
            return result;
        }

        public static void Print(string s) => Console.WriteLine(s);

        public static bool TryGetArgument(string[] args, Flag flag, out string result)
        {
            result = "";
            int flagindex = 0;
            if (args == null || args.Length == 0) return false;
            if (!(args.Contains(flag.Simplified) || args.Contains(flag.Name))) return false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == flag.Simplified || args[i] == flag.Name)
                {
                    flagindex = i;
                    break;
                }
            }
            try
            {
                result = args[flagindex + 1];
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool TryGetArgument(string[] args, Flag flag, out double result)
        {
            result = default;
            string resultstr;
            int flagindex = 0;
            if (args == null || args.Length == 0) return false;
            if (!(args.Contains(flag.Simplified) || args.Contains(flag.Name))) return false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == flag.Simplified || args[i] == flag.Name)
                {
                    flagindex = i;
                    break;
                }
            }
            try
            {
                resultstr = args[flagindex + 1];
                result = double.Parse(resultstr);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

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

        public static Blockchain ParseBlockchain(string path)
        {
            using BlockchainParser parser = new();
            using FileStream stream = new(path, FileMode.Open);
            Blockchain blockchain = parser.DeserializeBlockchain(stream);
            if (blockchain == null)
            {
                Utils.Print("Failed to load blockchain!");
                Environment.Exit(3);
                return null;
            }
            return blockchain;
        }
    }
}
