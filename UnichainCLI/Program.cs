using System.IO;
namespace Unichain.Cli;


public class Program
{
    public static void Main(string[] args)
    {
        if(args.Length == 0)
        {
            Console.WriteLine("Type -h for help");
            return;
        }
        if(args.Contains("-h") || args.Contains("--help"))
        {
            ShowHelp();
            return;
        }
        string chainpath = "";
        if(!TryGetChainPath(args, out chainpath))
        {
            Console.WriteLine("Provide a valid path to the chain file, or use 'unichain create' to create a new file.");
            return;
        }
        Console.WriteLine(chainpath);
    }
    public static void Print(string s)
    {
        Console.WriteLine(s);
    }

    public static bool TryGetChainPath(string[] args, out string filepath)
    {
        filepath = "";
        int flagindex = 0;
        if (args == null || args.Length == 0) return false;
        if (!(args.Contains("-f") || args.Contains("-file"))) return false;
        for (int i = 0; i < args.Length; i++)
        {
            if(args[i]=="-f" || args[i] == "-file")
            {
                flagindex = i;
            }
        }
        try
        {
            filepath = args[flagindex + 1];
        }catch (Exception)
        {
            return false;
        }
        if (!File.Exists(filepath)) return false;
        filepath = Path.GetFullPath(filepath);
        if (Path.GetExtension(filepath) != ".json")return false;
        return true;
    }

    public static void ShowHelp()
    {
        Print(@"
Welcome to the UniChain CLI!

Possible arguments:
  -h  --help => Display this help menu
  -f  --file => Path to the json file that the blockchain is stored");
    }
}
