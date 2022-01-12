﻿using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Unichain.Core;

namespace Unichain.Cli;


public class Program
{
    public static Dictionary<string, bool> Commands { get; set; } = new()
    {
        { "create", false },
        { "connect", true },
        { "add", true },
        { "mine", true },
        { "generate", false },
        { "get", true},
        { "print", true},
    };

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
        if(args.Contains("-h") || args.Contains("--help") || args[0]=="help")
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
            if (args[i] == "-f" || args[i] == "--file")
            {
                flagindex = i;
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
        if (Path.GetExtension(filepath) != ".json") return false;
        return true;
    }

    public static void CreateChain(string path)
    {
        Blockchain blockchain = new();
        SaveBlockChain(path, blockchain);
    }

    public static bool TryGetNumber(string[] args, out int number)
    {
        number = 0;
        int flagindex = 0;
        if (args == null || args.Length == 0) return false;
        if (!(args.Contains("-n") || args.Contains("--number"))) return false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-n" || args[i] == "--number")
            {
                flagindex = i;
            }
        }
        string? strnum;
        try
        {
            strnum = args[flagindex + 1];
        }
        catch (Exception)
        {
            return false;
        }
        if(strnum==null)return false;
        number = Convert.ToInt32(strnum);
        return true;
    }

    public static Blockchain ParseBlockchain(string path)
    {
        string data;
        try
        {
            data = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Print(ex.Message);
            Environment.Exit(2);
            return null;
        }
        //byte[] bytes = Convert.FromBase64String(data);

        //using MemoryStream ms = new(bytes);
        //using BsonDataReader reader = new(ms);            //this is in BSON

        //JsonSerializer serializer = new();
        //Blockchain? blockchain = serializer.Deserialize<Blockchain>(reader);
        var blockchain = JsonConvert.DeserializeObject<Blockchain>(data); //this is in plain JSON

        if (blockchain == null)
        {
            Print("Failed to load blockchain!");
            Environment.Exit(3);
            return null;
        }
        return blockchain;
    }

    public static void SaveBlockChain(string path, Blockchain blockchain)
    {
        //using MemoryStream ms = new();
        //using BsonDataWriter writer = new(ms);

        //JsonSerializer serializer = new();            //this is using BSON
        //serializer.Serialize(writer, blockchain);

        //string data = Convert.ToBase64String(ms.ToArray());
        string data = JsonConvert.SerializeObject(blockchain, Formatting.Indented); // this is in plain json

        try
        {
            File.WriteAllText(path, data);
        }
        catch (Exception ex)
        {
            Print(ex.Message);
            Environment.Exit(2);
            return;
        }
    }

    public static bool TryGetAddress(string[] args, out Address? address)
    {
        address = null;
        int flagindex = 0;
        if (args == null || args.Length == 0) return false;
        if (!(args.Contains("-a") || args.Contains("--address"))) return false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-a" || args[i] == "--address")
            {
                flagindex = i;
            }
        }
        string? straddr;
        try
        {
            straddr = args[flagindex + 1];
        }
        catch (Exception)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(straddr)) return false;
        try
        {
            address = new Address(straddr);
        }
        catch (Exception ex)
        {
            Print(ex.Message);
            Environment.Exit(4);
            return false;
        }
        return true;
    }

    public static void ShowHelp(string subcommand = "")
    {
        Print("Welcome to the UniChain CLI Helper!");
        switch (subcommand)
        {
            case "print":
                Print(@"
Possible flags for 'print' sub-command:
  -f  --file    => Path to the json file that the blockchain is stored
  -h  --hexadec => Prints out the chain in hexadecimal(JSON text bytes)
  -b  --binary  => Prints out the chain in binary(JSON text bytes)
  -o  --octal   => Prints out the chain in octal(JSON text bytes)
      --base64  => Prints out the chain in base 64(JSON text bytes)");
                break;
            case "mine":
                Print(@"
Possible flags for 'mine' sub-command:
  -a  --address => Sets the address that will receive the coins
  -f  --file    => Path to the json file that the blockchain is stored");
                break;
            case "generate":
                Print(@"
Possible flags for 'generate' sub-command:
  -n  --number => Sets the number of addresses and users to generate, defaults to 1
  -d  --dump   => Dumps all information to a text file");
                break;
            case "get":
                Print(@"
Possible flags and sub-commands for 'get' sub-command:
  -f  --file    => Path to the json file that the blockchain is stored
  -a  --address => The address to be searched, works with '--nft' and '--balance'
      --balance => Gets the token balance of a address
      --nft     => Gets all NFTs a address has");
                break;
            default:
                Print(@"
Possible arguments and sub-commands:
      create   => Creates a new blockchain in a file
      connect  => Connects the current pc to the network
      add      => Add a event to the pending transactions
      mine     => Mine all pending transactions
      generate => Generate a number of key pairs (Users and Addresses)
      get      => Search for something inside the blockchain
  -h  --help   => Display this help menu
  -f  --file   => Path to the json file that the blockchain is stored");
                break;
        }

    }
}
