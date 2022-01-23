using Newtonsoft.Json;
using System.Text;
using Unichain.CLI.Commands;
using Unichain.Core;
using Unichain.Parsing;

namespace Unichain.CLI;


public class Program
{
    public static Dictionary<string, bool> Commands { get; set; } = new()
    {
        // defines commands and if they need file argument or not
        { "create", false },
        { "connect", true },
        { "add", true },
        { "mine", true },
        { "generate", false },
        { "get", true },
        { "print", true },
    };

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Type -h for help");
            return;
        }

        // CHECKING FOR HELP
        if (Utils.HasFlag(args, ("-h", "--help")))
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

        var ispathinput = Utils.TryGetChainPath(args, out string path);


        // CHECK FOR FILELESS SUBCOMMAND          (it's separate because it doesn't use a path file)
        if (Commands.ContainsKey(args[0]))
        {
            if (!Commands[args[0]])
            {
                //does not need file, process them here!
                switch (args[0])
                {
                    #region create
                    case "create":
                        Create.Exec(path);
                        return;
                    #endregion

                    #region generate
                    case "generate":
                        Generate.Exec(args);
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
            Utils.Print("Please provide a valid path for the .json file containing the blockchain data using the '-f' flag.");
            Environment.Exit(3);
            return;
        }


        // CHECKING OTHER SUB COMMANDS
        switch (args[0])
        {
            #region mine
            case "mine":
                Mine.Exec(args,path);
                break;
            #endregion
            #region print
            case "print":
                Print.Exec(args, path);
                break;
            #endregion
            #region add
            case "add":

                break;
            #endregion
            #region get
            case "get":
                var bc = Utils.ParseBlockchain(path);
                bool isbalance = args.Contains("--balance");
                bool isnft = args.Contains("--nft");
                bool hasaddress = Utils.TryGetArgument(args,new("address","a"), out string addressStr);
                Address address = new(addressStr);
                if (hasaddress)
                {
                    if (isbalance)
                    {
                        Utils.Print($"{bc.GetBalance(address)}");
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
    
    public static void ShowAddHelp(EventType type)
    {
        switch (type)
        {
            case EventType.Transaction:
                Utils.Print(@"
Flags for Transaction event:
      --amount   => It's the amount of money that will be sent
      --receiver => The address that will receive the money");
                break;
            case EventType.NFTTransfer:
                Utils.Print(@"
Flags for NFT Transfer event:
  -i  --id       => The Id(Guid) of the NFT that will be transferred
      --receiver => The address that will receive the NFT");
                break;
            case EventType.NFTBurn:
                Utils.Print(@"
Flags for NFT Burn event:
  -i  --id => The Id(Guid) of the NFT that will be burned");
                break;
            case EventType.NFTMint:
                Utils.Print(@"
Flags for NFT Mint event:
  -m  --meta => The path to the .json file containing the metadata");
                break;
            case EventType.PoolOpen:
                Utils.Print(@"
Flags for Pool Open event:
  -m  --meta => The path to the .json file containing the metadata");
                break;
            case EventType.PoolVote:
                Utils.Print(@"
Flags for the Pool Vote event:
  -i  --id   => The Id(Guid) of the Pool that will receive the vote
  -v  --vote => The index of the vote cast");
                break;
            case EventType.DocumentSubmit:
                break;
            case EventType.MessageSendUser:
                break;
            default:
                break;
        }
    }

    public static void ShowHelp(string subcommand = "", EventType? type = null)
    {
        Utils.Print("Welcome to the UniChain CLI Helper!");
        switch (subcommand)
        {
            case "add":
                Utils.Print(@"
Mandatory flags for 'add' sub-command:
  -u  --user      => The private key that will sign the event
  -t  --type      => Flags the type of the event to be added, accepted values:
                     ['transaction', 'nftmint', 'nfttransfer', 'nftburn',
                      'poolopen', 'poolvote', 'msgsenduser', 'msgsendgroup',
                      'docsubmit']");
                if (type != null) ShowAddHelp(type.GetValueOrDefault());
                break;
            case "print":
                Utils.Print(@"
Possible flags for 'Utils.Print' sub-command:
  -f  --file    => Path to the json file that the blockchain is stored
  -d  --dump    => Dumps the output to a file
  -x  --hex     => Utils.Prints out the chain in hexadecimal(JSON text bytes)
      --binary  => Utils.Prints out the chain in binary(JSON text bytes)
      --octal   => Utils.Prints out the chain in octal(JSON text bytes)
  -b  --base64  => Utils.Prints out the chain in base 64(JSON text bytes)");
                break;
            case "mine":
                Utils.Print(@"
Possible flags for 'mine' sub-command:
  -a  --address => Sets the address that will receive the coins
  -f  --file    => Path to the json file that the blockchain is stored");
                break;
            case "generate":
                Utils.Print(@"
Possible flags for 'generate' sub-command:
  -n  --number => Sets the number of addresses and users to generate, defaults to 1
  -d  --dump   => Dumps all information to a text file");
                break;
            case "get":
                Utils.Print(@"
Possible flags and options for 'get' sub-command:
  -f  --file    => Path to the .chain file that the blockchain is stored
  -a  --address => The address to be searched, works with '--nft' and '--balance'
      --balance => Gets the token balance of a address
      --nft     => Gets all NFTs a address has");
                break;
            case "cache":
                Utils.Print(@"
Possible flags for 'cache' sub-command:
  ");
                break;
            case "create":
                Utils.Print(@"
Possible flags for 'create' sub-command:
  -f  --file => Path to the .chain file that will be created");
                break;
            default:
                Utils.Print(@"
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
