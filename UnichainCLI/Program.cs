using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Text;
using System.Text.Unicode;
using Unichain.Core;
using Unichain.Parsing;

namespace Unichain.CLI;


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
        if (args.Length == 0)
        {
            Console.WriteLine("Type -h for help");
            return;
        }

        // CHECKING FOR HELP
        if (args.Contains("-h") || args.Contains("--help") || args[0] == "help")
        {
            if (Commands.ContainsKey(args[0]))
            {
                // IS SPECIFIC HELP
                ShowHelp(args[0]);
            }
            else
            {
                ShowHelp();
            }
            return;
        }

        var ispathinput = TryGetChainPath(args, out string path);


        // CHECK FOR FILELESS SUBCOMMAND          (it's separate because it doesn't use a path file)
        if (Commands.ContainsKey(args[0])){
            if (!Commands[args[0]])
            {
                //does not need file, process them here!
                switch (args[0])
                {
                    #region create
                    case "create":
                        if (path=="") // it doesn't work using ispathinput
                        {
                            //path not found
                            Console.WriteLine("No parameter for the file location found, do you want to create one" +
                                " in the current directory?[y/N]");
                            var input = Console.ReadLine();
                            if (input?.ToUpper() != "Y")
                            {
                                Print("Exitting...");
                                Environment.Exit(0);
                            }
                            path = Environment.CurrentDirectory + "\\unichain.json";
                            CreateChain(path);
                            Print($"Blockchain created in {path}");
                            Environment.Exit(0);
                        }
                        //found path
                        CreateChain(path);
                        Print($"Blockchain created in {path}");
                        Environment.Exit(0);
                        return;
                    #endregion

                    #region generate
                    case "generate":
                        var foundnum = TryGetNumber(args, out int number);
                        var isdump = args.Contains("-d") || args.Contains("--dump");
                        string dumppath = $"{Environment.CurrentDirectory}\\dumpfile-{DateTime.Now.Ticks}.yml";
                        using (var stream = File.AppendText(dumppath))
                        {
                            if (!foundnum || number == 0)
                            {
                                number = 1;
                            }
                            foreach (var chunk in Enumerable.Range(1, number).Chunk(50))
                            {
                                List<(User user, int index)> users = new();
                                for (int i = 0; i < number; i++)
                                {
                                    User user = new();
                                    if (!isdump) Print($"User nº{i} - \n    PrivateKey: {user.GetPrivateKey().Key}\n    PublicKey: {user.Address}");
                                    users.Add((user, i));
                                }
                                if (isdump)
                                {

                                    foreach (var userchunk in users.Chunk(50))
                                    {
                                        string textchunk = "";
                                        foreach (var user in userchunk)
                                        {
                                            textchunk += $"{user.index}:\n privkey: {user.user.GetPrivateKey().Key}\n publkey: {user.user.Address}\n";
                                        }
                                        stream.Write(textchunk);
                                    }
                                }
                            }
                        }
                        Environment.Exit(0);
                        return;
                    #endregion
                    default:
                        break;
                }
            }
        }



        // CHECK FOR PATH AVAILABILITY
        if (!ispathinput)
        {
            Print("Please provide a valid path for the .json file containing the blockchain data using the '-f' flag.");
            Environment.Exit(3);
            return;
        }


        // CHECKING OTHER SUB COMMANDS
        switch (args[0])
        {
            #region mine
            case "mine":
                var bc = ParseBlockchain(path);
                var isaddrfound = TryGetAddress(args, out var mineraddress);
                if (!isaddrfound)
                {
                    Print("Please provide a address to receive the miner reward!");
                    Environment.Exit(5);
                    return;
                }
                Print($"Mining with this address: {mineraddress}");
                bc.MinePendingTransactions(mineraddress);
                Print($"Mined sucessfully! Received {bc.Reward} tokens");

                //save chain
                SaveBlockChain(path, bc);
                Environment.Exit(0);
                break;
            #endregion
            #region print
            case "print":
                bc = ParseBlockchain(path);
                bool isbinary = args.Contains("--binary");
                bool isoctal = args.Contains("--octal");
                bool ishex = args.Contains("-x") || args.Contains("--hex");
                bool isb64 = args.Contains("-b") || args.Contains("--base64");
                bool isdump = args.Contains("-d") || args.Contains("--dump");
                string dumppath = $"{Environment.CurrentDirectory}\\dumpfile-{DateTime.Now.Ticks}.txt";
                string rawjson = JsonConvert.SerializeObject(bc, Formatting.Indented);

                if (isbinary)
                {
                    throw new NotImplementedException();
                }else if (isoctal)
                {
                    throw new NotImplementedException();
                }else if (ishex)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(rawjson);
                    rawjson = Convert.ToHexString(bytes);
                }else if (isb64)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(rawjson);
                    rawjson = Convert.ToBase64String(bytes);
                }
                if (isdump)
                {
                    try
                    {
                        File.WriteAllText(dumppath, rawjson);
                    }catch(Exception ex)
                    {
                        Print(ex.Message);
                        Environment.Exit(2);
                    }
                }
                else
                {
                    Print(rawjson);
                }
                Environment.Exit(0);
                break;
            #endregion
            #region add
            case "add":

                break;
            #endregion
            #region get
            case "get":
                bc = ParseBlockchain(path);
                bool isbalance = args.Contains("--balance");
                bool isnft = args.Contains("--nft");
                bool hasaddress = TryGetAddress(args, out Address? address);
                if (hasaddress)
                {
                    if (isbalance)
                    {
                        Print($"{bc.GetBalance(address)}");
                        Environment.Exit(0);
                    }
                    if (isnft)
                    {
                        throw new NotImplementedException();
                    }
                }
                break;
            #endregion
            default:
                break;
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
        BlockchainParser parser = new();
        using FileStream stream = new(path, FileMode.Open);
        Blockchain blockchain = parser.DeserializeBlockchain(stream);
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
        BlockchainParser parser = new();
        using MemoryStream ms = parser.SerializeBlockchain(blockchain);
        try
        {
            File.WriteAllBytes(path, ms.ToArray());
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
            case "add":
                Print(@"
Possible flags for 'add' sub-command:
  -u  --user      => The private key that will sign the event

Use-case variable flags for 'add' sub-command:
  -a  --address   => The receiver(if the event doesn't need a receiver, it'll be ignored
      --amount    => Amount of a transaction
  -m  --metadata  => The path for a metadata file, it must comply to respective metadata classes

Possible options for 'add' sub-command:
      transaction => Creates a new transaction on the blockchain
      nftmint     => Mints a new NFT in the blockchain
      nfttransfer => Transfer the ownership of a NFT for other address
      nftburn     => Burn a already existing NFT");
                break;
            case "print":
                Print(@"
Possible flags for 'print' sub-command:
  -f  --file    => Path to the json file that the blockchain is stored
  -d  --dump    => Dumps the output to a file
  -x  --hex     => Prints out the chain in hexadecimal(JSON text bytes)
      --binary  => Prints out the chain in binary(JSON text bytes)
      --octal   => Prints out the chain in octal(JSON text bytes)
  -b  --base64  => Prints out the chain in base 64(JSON text bytes)");
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
Possible flags and options for 'get' sub-command:
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
