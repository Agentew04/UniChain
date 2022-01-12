using System.IO;
using Newtonsoft.Json;

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


        // CHECKING SUB COMMANDS
        if (args[0] == "create")
        {
            if (!TryGetChainPath(args, out string path))
            {
                //path not found
                Console.WriteLine("No parameter for the file location found, do you want to create one" +
                    " in the current directory?[Y/N]");
                var input = Console.ReadLine();
                if (input?.ToUpper() != "Y")
                {
                    Print("Exitting...");
                    return;
                }
                path = Environment.CurrentDirectory + "\\unichain.json";
                CreateChain(path);
                Print($"Blockchain created in {path}");
                return;
            }
            //found path
            CreateChain(path);
            Print($"Blockchain created in {path}");
            return;
        }



        // CHECKING FLAGS
        if(args.Contains("-h") || args.Contains("--help"))
        {
            ShowHelp();
            return;
        }



        if(!TryGetChainPath(args, out string chainpath))
        {
            Console.WriteLine("Provide a valid path to the chain file, or use 'unichain create' to create a new file.");
            return;
        }





        Environment.Exit(0);
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

    public static void CreateChain(string path)
    {
        Blockchain blockchain = new();
        var jsonserialized = JsonConvert.SerializeObject(blockchain, Formatting.Indented);
        try
        {
            File.WriteAllText(path, jsonserialized);
        }catch (Exception e)
        {
            Print(e.Message);
            Environment.Exit(1);
        }
    }

    public static void ShowHelp()
    {
        Print(@"
Welcome to the UniChain CLI!

Possible arguments:
  -h  --help  => Display this help menu
  -f  --file  => Path to the json file that the blockchain is stored
      create  => Creates a new blockchain in a file");
    }
}
